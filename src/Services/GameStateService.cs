using System.Text.Json;
using Legendary.Companion.Data;
using Legendary.Companion.Models;
using Microsoft.JSInterop;

namespace Legendary.Companion.Services;

/// <summary>
/// Holds the app's live state (player count, which sets are enabled, the current
/// generated setup) and persists user preferences to browser local storage.
/// UI components subscribe to <see cref="OnChange"/> to re-render.
/// </summary>
public sealed class GameStateService
{
    private const string PlayersKey = "legendary.players";
    private const string EnabledSetsKey = "legendary.enabledSets";

    private readonly IJSRuntime _js;
    private readonly SetupRandomizer _randomizer;
    private readonly HashSet<string> _enabledSetIds;

    private bool _initialized;

    public GameStateService(IJSRuntime js, SetupRandomizer randomizer)
    {
        _js = js;
        _randomizer = randomizer;
        _enabledSetIds = SetRegistry.AllSets
            .Where(s => s.EnabledByDefault)
            .Select(s => s.Id)
            .ToHashSet();
    }

    public int Players { get; private set; } = 2;
    public GameSetup? Setup { get; private set; }

    public event Action? OnChange;

    public IReadOnlyList<CardSet> AllSets => SetRegistry.AllSets;
    public bool IsEnabled(string setId) => _enabledSetIds.Contains(setId);
    public CardPool CurrentPool => CardPool.From(SetRegistry.AllSets.Where(s => _enabledSetIds.Contains(s.Id)));

    /// <summary>Load persisted preferences. Safe to call more than once.</summary>
    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;

        try
        {
            var players = await _js.InvokeAsync<string?>("localStorage.getItem", PlayersKey);
            if (int.TryParse(players, out var p) && p is >= 1 and <= 5)
                Players = p;

            var setsJson = await _js.InvokeAsync<string?>("localStorage.getItem", EnabledSetsKey);
            if (!string.IsNullOrWhiteSpace(setsJson))
            {
                var ids = JsonSerializer.Deserialize<List<string>>(setsJson);
                if (ids is not null)
                {
                    _enabledSetIds.Clear();
                    foreach (var id in ids.Where(id => SetRegistry.FindById(id) is not null))
                        _enabledSetIds.Add(id);
                }
            }
        }
        catch
        {
            // Local storage may be unavailable (private mode, prerender) — fall back to defaults.
        }

        NotifyChanged();
    }

    public async Task SetPlayersAsync(int players)
    {
        players = Math.Clamp(players, 1, 5);
        if (players == Players) return;
        Players = players;
        await PersistAsync(PlayersKey, Players.ToString());
        NotifyChanged();
    }

    public async Task SetEnabledAsync(string setId, bool enabled)
    {
        if (SetRegistry.FindById(setId) is null) return;

        if (enabled) _enabledSetIds.Add(setId);
        else _enabledSetIds.Remove(setId);

        // Never allow an unplayable (empty) pool.
        if (!CurrentPool.IsPlayable)
        {
            _enabledSetIds.Add(setId);
            return;
        }

        await PersistAsync(EnabledSetsKey, JsonSerializer.Serialize(_enabledSetIds.ToList()));
        NotifyChanged();
    }

    /// <summary>Generate a brand-new full setup.</summary>
    public void Randomize()
    {
        var pool = CurrentPool;
        if (!pool.IsPlayable) return;
        Setup = _randomizer.Generate(Players, pool);
        NotifyChanged();
    }

    /// <summary>Reroll just one category, keeping the rest stable.</summary>
    public void Reroll(CardCategory category)
    {
        if (Setup is null) return;
        var pool = CurrentPool;
        if (!pool.IsPlayable) return;
        Setup = _randomizer.Reroll(Setup, category, pool);
        NotifyChanged();
    }

    private async Task PersistAsync(string key, string value)
    {
        try { await _js.InvokeVoidAsync("localStorage.setItem", key, value); }
        catch { /* ignore storage failures */ }
    }

    private void NotifyChanged() => OnChange?.Invoke();
}
