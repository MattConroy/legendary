using System.Text.Json;
using Legendary.Companion.Data;
using Legendary.Companion.Models;
using Microsoft.JSInterop;

namespace Legendary.Companion.Services;

public enum SetSort { Name, Date }

/// <summary>
/// Holds the app's live state (player count, which sets are owned/enabled, the
/// current setup, the Sets-page sort) and persists preferences to local storage.
/// UI components subscribe to <see cref="OnChange"/> to re-render.
/// </summary>
public sealed class GameStateService
{
    private const string PlayersKey = "legendary.players";
    private const string EnabledSetsKey = "legendary.enabledSets";
    private const string OwnedSetsKey = "legendary.ownedSets";
    private const string SortKey = "legendary.setSort";

    private readonly IJSRuntime _js;
    private readonly SetupRandomizer _randomizer;
    private readonly HashSet<string> _ownedSetIds;
    private readonly HashSet<string> _enabledSetIds;

    private bool _initialized;

    public GameStateService(IJSRuntime js, SetupRandomizer randomizer)
    {
        _js = js;
        _randomizer = randomizer;
        var defaults = SetRegistry.AllSets.Where(s => s.EnabledByDefault).Select(s => s.Id).ToHashSet();
        _ownedSetIds = new HashSet<string>(defaults);
        _enabledSetIds = new HashSet<string>(defaults);
    }

    public int Players { get; private set; } = 2;
    public GameSetup? Setup { get; private set; }
    public SetSort Sort { get; private set; } = SetSort.Date;
    public bool SortDescending { get; private set; }

    public event Action? OnChange;

    public IReadOnlyList<CardSet> AllSets => SetRegistry.AllSets;

    /// <summary>Sets ordered by the current sort key and direction.</summary>
    public IReadOnlyList<CardSet> SortedSets
    {
        get
        {
            IEnumerable<CardSet> q = Sort == SetSort.Name
                ? AllSets.OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
                : AllSets.OrderBy(s => s.Released).ThenBy(s => s.Name, StringComparer.OrdinalIgnoreCase);
            return (SortDescending ? q.Reverse() : q).ToList();
        }
    }

    public bool IsOwned(string setId) => _ownedSetIds.Contains(setId);
    public bool IsEnabled(string setId) => _enabledSetIds.Contains(setId);
    public int OwnedCount => _ownedSetIds.Count;

    /// <summary>
    /// The randomiser draws from every *enabled* set. Enabling is independent of
    /// ownership (you might be borrowing a set), though owning one enables it.
    /// </summary>
    public CardPool CurrentPool =>
        CardPool.From(SetRegistry.AllSets.Where(s => _enabledSetIds.Contains(s.Id)));

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;

        try
        {
            var players = await _js.InvokeAsync<string?>("localStorage.getItem", PlayersKey);
            if (int.TryParse(players, out var p) && p is >= 1 and <= 5)
                Players = p;

            await LoadSetIdsAsync(OwnedSetsKey, _ownedSetIds);
            await LoadSetIdsAsync(EnabledSetsKey, _enabledSetIds);

            var sort = await _js.InvokeAsync<string?>("localStorage.getItem", SortKey);
            if (!string.IsNullOrWhiteSpace(sort))
            {
                var parts = sort.Split(':');
                if (Enum.TryParse<SetSort>(parts[0], ignoreCase: true, out var key)) Sort = key;
                SortDescending = parts.Length > 1 && parts[1] == "desc";
            }
        }
        catch
        {
            // Local storage may be unavailable (private mode) — keep defaults.
        }

        NotifyChanged();
    }

    private async Task LoadSetIdsAsync(string key, HashSet<string> target)
    {
        var json = await _js.InvokeAsync<string?>("localStorage.getItem", key);
        if (string.IsNullOrWhiteSpace(json)) return;
        var ids = JsonSerializer.Deserialize<List<string>>(json);
        if (ids is null) return;
        target.Clear();
        foreach (var id in ids.Where(id => SetRegistry.FindById(id) is not null))
            target.Add(id);
    }

    public async Task SetPlayersAsync(int players)
    {
        players = Math.Clamp(players, 1, 5);
        if (players == Players) return;
        Players = players;
        await PersistAsync(PlayersKey, Players.ToString());
        NotifyChanged();
    }

    /// <summary>
    /// Mark a set as owned or not. Owning a set also enables it; un-owning leaves
    /// the play toggle alone (you might still be borrowing it).
    /// </summary>
    public async Task SetOwnedAsync(string setId, bool owned)
    {
        if (SetRegistry.FindById(setId) is null) return;

        if (owned)
        {
            _ownedSetIds.Add(setId);
            _enabledSetIds.Add(setId);
            await PersistAsync(EnabledSetsKey, Serialize(_enabledSetIds));
        }
        else
        {
            _ownedSetIds.Remove(setId);
        }

        await PersistAsync(OwnedSetsKey, Serialize(_ownedSetIds));
        NotifyChanged();
    }

    /// <summary>Enable/disable a set for the next game (independent of ownership).</summary>
    public async Task SetEnabledAsync(string setId, bool enabled)
    {
        if (SetRegistry.FindById(setId) is null) return;

        if (enabled) _enabledSetIds.Add(setId);
        else _enabledSetIds.Remove(setId);

        await PersistAsync(EnabledSetsKey, Serialize(_enabledSetIds));
        NotifyChanged();
    }

    /// <summary>Enable or disable every set.</summary>
    public async Task SetAllEnabledAsync(bool enabled)
    {
        _enabledSetIds.Clear();
        if (enabled) _enabledSetIds.UnionWith(SetRegistry.AllSets.Select(s => s.Id));
        await PersistAsync(EnabledSetsKey, Serialize(_enabledSetIds));
        NotifyChanged();
    }

    /// <summary>Add all owned sets to the play pool, or remove all owned sets from it.</summary>
    public async Task SetOwnedEnabledAsync(bool enabled)
    {
        if (enabled) _enabledSetIds.UnionWith(_ownedSetIds);
        else _enabledSetIds.ExceptWith(_ownedSetIds);
        await PersistAsync(EnabledSetsKey, Serialize(_enabledSetIds));
        NotifyChanged();
    }

    public async Task SetSortAsync(SetSort key)
    {
        if (Sort == key) SortDescending = !SortDescending;
        else { Sort = key; SortDescending = false; }
        await PersistAsync(SortKey, $"{Sort}:{(SortDescending ? "desc" : "asc")}".ToLowerInvariant());
        NotifyChanged();
    }

    public void Randomize()
    {
        var pool = CurrentPool;
        if (!pool.IsPlayable) return;
        Setup = _randomizer.Generate(Players, pool);
        NotifyChanged();
    }

    public void Reroll(CardCategory category)
    {
        if (Setup is null) return;
        var pool = CurrentPool;
        if (!pool.IsPlayable) return;
        Setup = _randomizer.Reroll(Setup, category, pool);
        NotifyChanged();
    }

    private static string Serialize(HashSet<string> ids) => JsonSerializer.Serialize(ids.ToList());

    private async Task PersistAsync(string key, string value)
    {
        try { await _js.InvokeVoidAsync("localStorage.setItem", key, value); }
        catch { /* ignore storage failures */ }
    }

    private void NotifyChanged() => OnChange?.Invoke();
}
