using Legendary.Companion.Abstractions;
using Legendary.Companion.Models;

namespace Legendary.Companion.Services;

public enum SetSort { Name, Date }

/// <summary>
/// Holds the app's live state (player count, which sets are owned/enabled, the
/// current setup, the Sets-page sort) and persists preferences through an
/// <see cref="IPreferenceRepository"/>. Content sets come from an
/// <see cref="ISetRepository"/>. UI components subscribe to <see cref="OnChange"/>
/// to re-render.
/// </summary>
public sealed class GameStateService
{
    private const string DefaultPlayersKey = "legendary.players";
    private const string EnabledSetsKey = "legendary.enabledSets";
    private const string OwnedSetsKey = "legendary.ownedSets";
    private const string SortKey = "legendary.setSort";
    private const string TargetKey = "legendary.difficultyTarget";

    private readonly SetupRandomizer _randomizer;
    private readonly ISetRepository _sets;
    private readonly IPreferenceRepository _prefs;
    private readonly HashSet<string> _ownedSetIds = [];
    private readonly HashSet<string> _enabledSetIds = [];

    private bool _initialized;

    public GameStateService(SetupRandomizer randomizer, ISetRepository sets, IPreferenceRepository prefs)
    {
        _randomizer = randomizer;
        _sets = sets;
        _prefs = prefs;
    }

    /// <summary>Player count for the current game. Starts from <see cref="DefaultPlayers"/>
    /// and can be changed per-game on the setup screen without altering the saved default.</summary>
    public int Players { get; private set; } = 2;

    /// <summary>The saved default player count, edited on the Sets &amp; Options screen.</summary>
    public int DefaultPlayers { get; private set; } = 2;

    public GameSetup? Setup { get; private set; }
    public SetSort Sort { get; private set; } = SetSort.Date;
    public bool SortDescending { get; private set; }

    /// <summary>Difficulty the randomiser aims for; null means "Any" (no bias).</summary>
    public DifficultyBand? Target { get; private set; }

    public event Action? OnChange;

    public IReadOnlyList<CardSet> AllSets => _sets.Sets;

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

    /// <summary>The randomiser draws from every enabled set (owned or borrowed).</summary>
    public CardPool CurrentPool =>
        CardPool.From(_sets.Sets.Where(s => _enabledSetIds.Contains(s.Id)));

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;

        await _sets.EnsureLoadedAsync();

        // Defaults from the loaded catalog, then override from stored preferences.
        foreach (var id in _sets.Sets.Where(s => s.EnabledByDefault).Select(s => s.Id))
        {
            _ownedSetIds.Add(id);
            _enabledSetIds.Add(id);
        }

        if (int.TryParse(await _prefs.GetAsync(DefaultPlayersKey), out var p) && p is >= 1 and <= 5)
        {
            DefaultPlayers = p;
            Players = p; // the current game starts at the saved default
        }

        await LoadSetIdsAsync(OwnedSetsKey, _ownedSetIds);
        await LoadSetIdsAsync(EnabledSetsKey, _enabledSetIds);

        var sort = await _prefs.GetAsync(SortKey);
        if (!string.IsNullOrWhiteSpace(sort))
        {
            var parts = sort.Split(':');
            if (Enum.TryParse<SetSort>(parts[0], ignoreCase: true, out var key)) Sort = key;
            SortDescending = parts.Length > 1 && parts[1] == "desc";
        }

        if (Enum.TryParse<DifficultyBand>(await _prefs.GetAsync(TargetKey), ignoreCase: true, out var band))
            Target = band;

        NotifyChanged();
    }

    private async Task LoadSetIdsAsync(string key, HashSet<string> target)
    {
        var ids = await _prefs.GetListAsync(key);
        if (ids is null) return; // never saved — keep the defaults
        target.Clear();
        foreach (var id in ids.Where(id => _sets.FindById(id) is not null))
            target.Add(id);
    }

    /// <summary>Change the player count for the current game only — not persisted,
    /// so it never overwrites the saved default.</summary>
    public void SetPlayers(int players)
    {
        players = Math.Clamp(players, 1, 5);
        if (players == Players) return;
        Players = players;
        NotifyChanged();
    }

    /// <summary>Change the saved default player count and apply it to the current game.</summary>
    public async Task SetDefaultPlayersAsync(int players)
    {
        players = Math.Clamp(players, 1, 5);
        DefaultPlayers = players;
        Players = players;
        await _prefs.SetAsync(DefaultPlayersKey, players.ToString());
        NotifyChanged();
    }

    /// <summary>
    /// Mark a set as owned or not. Owning a set also enables it; un-owning leaves
    /// the play toggle alone (you might still be borrowing it).
    /// </summary>
    public async Task SetOwnedAsync(string setId, bool owned)
    {
        if (_sets.FindById(setId) is null) return;

        if (owned)
        {
            _ownedSetIds.Add(setId);
            _enabledSetIds.Add(setId);
            await _prefs.SetListAsync(EnabledSetsKey, _enabledSetIds);
        }
        else
        {
            _ownedSetIds.Remove(setId);
        }

        await _prefs.SetListAsync(OwnedSetsKey, _ownedSetIds);
        NotifyChanged();
    }

    /// <summary>Enable/disable a set for the next game (independent of ownership).</summary>
    public async Task SetEnabledAsync(string setId, bool enabled)
    {
        if (_sets.FindById(setId) is null) return;

        if (enabled) _enabledSetIds.Add(setId);
        else _enabledSetIds.Remove(setId);

        await _prefs.SetListAsync(EnabledSetsKey, _enabledSetIds);
        NotifyChanged();
    }

    /// <summary>Enable or disable every set.</summary>
    public async Task SetAllEnabledAsync(bool enabled)
    {
        _enabledSetIds.Clear();
        if (enabled) _enabledSetIds.UnionWith(_sets.Sets.Select(s => s.Id));
        await _prefs.SetListAsync(EnabledSetsKey, _enabledSetIds);
        NotifyChanged();
    }

    /// <summary>Add all owned sets to the play pool, or remove all owned sets from it.</summary>
    public async Task SetOwnedEnabledAsync(bool enabled)
    {
        if (enabled) _enabledSetIds.UnionWith(_ownedSetIds);
        else _enabledSetIds.ExceptWith(_ownedSetIds);
        await _prefs.SetListAsync(EnabledSetsKey, _enabledSetIds);
        NotifyChanged();
    }

    public async Task SetSortAsync(SetSort key)
    {
        if (Sort == key) SortDescending = !SortDescending;
        else { Sort = key; SortDescending = false; }
        await _prefs.SetAsync(SortKey, $"{Sort}:{(SortDescending ? "desc" : "asc")}".ToLowerInvariant());
        NotifyChanged();
    }

    public async Task SetTargetAsync(DifficultyBand? target)
    {
        Target = target;
        await _prefs.SetAsync(TargetKey, target?.ToString() ?? "");
        NotifyChanged();
    }

    public void Randomize()
    {
        var pool = CurrentPool;
        if (!pool.IsPlayable) return;
        Setup = GenerateForTarget(pool);
        NotifyChanged();
    }

    // Draws a setup, then (if a difficulty target is set) rerolls the Mastermind
    // and Scheme toward that band, keeping the closest match found. Best-effort:
    // if the enabled pool can't reach the target, it returns the nearest it saw.
    private GameSetup GenerateForTarget(CardPool pool)
    {
        var setup = _randomizer.Generate(Players, pool);
        if (Target is not { } target) return setup;

        var best = setup;
        var bestDist = BandDistance(best, target);
        for (var i = 0; i < 200 && bestDist > 0; i++)
        {
            setup = _randomizer.Reroll(setup, CardCategory.Mastermind, pool);
            setup = _randomizer.Reroll(setup, CardCategory.Scheme, pool);
            var dist = BandDistance(setup, target);
            if (dist < bestDist) { best = setup; bestDist = dist; }
        }
        return best;
    }

    private static int BandDistance(GameSetup setup, DifficultyBand target)
    {
        if (setup.Threat is not { } threat) return 0; // unrated — accept anything
        return Math.Abs((int)threat.Band - (int)target);
    }

    public void Reroll(CardCategory category)
    {
        if (Setup is null) return;
        var pool = CurrentPool;
        if (!pool.IsPlayable) return;
        Setup = _randomizer.Reroll(Setup, category, pool);
        NotifyChanged();
    }

    private void NotifyChanged() => OnChange?.Invoke();
}
