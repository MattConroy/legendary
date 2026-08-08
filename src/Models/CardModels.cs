namespace Legendary.Companion.Models;

/// <summary>
/// The five randomised setup categories in a Legendary game.
/// </summary>
public enum CardCategory
{
    Mastermind,
    Scheme,
    VillainGroup,
    Henchmen,
    Hero
}

/// <summary>
/// Base type shared by every card-group the randomiser can pick.
/// Everything the randomiser needs lives on this base type, so adding a new
/// expansion never requires changing the randomiser — it just contributes more
/// of these items to the pool.
/// </summary>
public abstract record GameCard
{
    /// <summary>Stable identifier, unique across all sets (e.g. "core:magneto").</summary>
    public required string Id { get; init; }

    /// <summary>Display name.</summary>
    public required string Name { get; init; }

    [System.Text.Json.Serialization.JsonIgnore]
    public abstract CardCategory Category { get; }
}

public sealed record Mastermind : GameCard
{
    public override CardCategory Category => CardCategory.Mastermind;

    /// <summary>
    /// The group this Mastermind "Always Leads". When this Mastermind is chosen,
    /// that group is forced into the Villain Deck (shown with a "Required" badge).
    /// May reference either a Villain Group or a Henchman group.
    /// </summary>
    public string? AlwaysLeadsGroupId { get; init; }

    /// <summary>Extra setup steps this Mastermind imposes (shown on its card).</summary>
    public IReadOnlyList<string> Notes { get; init; } = [];

    /// <summary>Editorial 1–5 difficulty rating (community/best-effort, tunable). Null if unrated.</summary>
    public int? Difficulty { get; init; }

    /// <summary>
    /// This Mastermind's base contribution to a setup's Threat: rating × 2 − 1
    /// (1, 3, 5, 7, 9), so it anchors the 1–10 scale. Null when unrated.
    /// </summary>
    public int? ThreatBase => Difficulty is { } d ? d * 2 - 1 : null;
}

public sealed record Scheme : GameCard
{
    public override CardCategory Category => CardCategory.Scheme;

    /// <summary>
    /// Setup modifications this Scheme imposes on the randomised categories,
    /// per the official Scheme setup text.
    /// </summary>
    public SchemeSetup Setup { get; init; } = SchemeSetup.None;

    /// <summary>Editorial 1–5 difficulty rating (community/best-effort, tunable). Null if unrated.</summary>
    public int? Difficulty { get; init; }

    /// <summary>
    /// This Scheme's small ±1 Threat modifier — its rating relative to an average
    /// of 3, clamped to [−1, +1], since scheme difficulty is contextual. Null when
    /// unrated (treated as no nudge).
    /// </summary>
    public int? ThreatModifier => Difficulty is { } d ? Math.Clamp(d - 3, -1, 1) : null;
}

/// <summary>
/// How a Scheme changes the standard setup, taken from its card. Only factual,
/// numeric/structural setup data is modelled here (counts, which groups are forced) —
/// not the card's ability wording.
/// </summary>
public sealed record SchemeSetup
{
    public static readonly SchemeSetup None = new();

    /// <summary>Scheme Twists in a standard game (default 8).</summary>
    public int Twists { get; init; } = 8;

    /// <summary>Per-player-count overrides for Twists (e.g. Super Hero Civil War).</summary>
    public IReadOnlyDictionary<int, int>? TwistsByPlayers { get; init; }

    /// <summary>Relative change to the number of Heroes in the Hero Deck.</summary>
    public int HeroDelta { get; init; }

    /// <summary>Absolute Hero count that overrides the per-player table (e.g. 6).</summary>
    public int? Heroes { get; init; }

    /// <summary>Per-player-count absolute Hero overrides (e.g. 4 Heroes at 2 players).</summary>
    public IReadOnlyDictionary<int, int>? HeroesByPlayers { get; init; }

    public int VillainGroupDelta { get; init; }
    public int HenchmenDelta { get; init; }

    /// <summary>Villain Groups this Scheme forces into the setup (marked Required).
    /// May reference groups from other sets (e.g. the Kree-Skrull War forcing the Core Skrulls).</summary>
    public IReadOnlyList<string> RequiredVillainGroupIds { get; init; } = [];

    /// <summary>Henchman Groups this Scheme forces into the setup (marked Required).</summary>
    public IReadOnlyList<string> RequiredHenchmenGroupIds { get; init; } = [];

    /// <summary>Bystanders in the Villain Deck when the Scheme overrides the default.</summary>
    public int? Bystanders { get; init; }

    /// <summary>Short factual setup notes surfaced to the player.</summary>
    public IReadOnlyList<string> Notes { get; init; } = [];

    // ----- how this scheme resolves each count from the base setup table -----
    // These apply only the scheme's own rules; clamping to the available pool and
    // honouring required groups is the randomiser's job (it alone knows the pool).

    /// <summary>Heroes in the Hero Deck at the given player count: a per-player
    /// override, an absolute override, or the base table plus this scheme's delta.</summary>
    public int HeroesFor(SetupRule rule, int players)
    {
        if (HeroesByPlayers is { } byPlayers && byPlayers.TryGetValue(players, out var v)) return v;
        if (Heroes is { } absolute) return absolute;
        return rule.Heroes + HeroDelta;
    }

    /// <summary>Villain Groups for this scheme (base table plus its delta).</summary>
    public int VillainGroupsFor(SetupRule rule) => rule.VillainGroups + VillainGroupDelta;

    /// <summary>Henchman Groups for this scheme (base table plus its delta).</summary>
    public int HenchmenFor(SetupRule rule) => rule.Henchmen + HenchmenDelta;

    /// <summary>Scheme Twists to shuffle in at the given player count.</summary>
    public int TwistsFor(int players) =>
        TwistsByPlayers is { } byPlayers && byPlayers.TryGetValue(players, out var v) ? v : Twists;

    /// <summary>Bystanders in the Villain Deck: this scheme's override, else the base rule.</summary>
    public int BystandersFor(SetupRule rule) => Bystanders ?? rule.Bystanders;
}

public sealed record VillainGroup : GameCard
{
    public override CardCategory Category => CardCategory.VillainGroup;
}

public sealed record Henchmen : GameCard
{
    public override CardCategory Category => CardCategory.Henchmen;
}

public sealed record Hero : GameCard
{
    public override CardCategory Category => CardCategory.Hero;

    /// <summary>Team affiliation for flavour/grouping (e.g. "Avengers", "X-Men").</summary>
    public string? Team { get; init; }
}
