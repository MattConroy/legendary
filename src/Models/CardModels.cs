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

    /// <summary>Id of the <see cref="CardSet"/> this card belongs to.</summary>
    public required string SetId { get; init; }

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

    /// <summary>Optional flavour tagline (not currently shown).</summary>
    public string? Tagline { get; init; }

    /// <summary>Extra setup steps this Mastermind imposes (shown on its card).</summary>
    public IReadOnlyList<string> Notes { get; init; } = [];
}

public sealed record Scheme : GameCard
{
    public override CardCategory Category => CardCategory.Scheme;

    /// <summary>
    /// Setup modifications this Scheme imposes on the randomised categories,
    /// per the official Scheme setup text.
    /// </summary>
    public SchemeSetup Setup { get; init; } = SchemeSetup.None;
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

    /// <summary>A Villain Group this Scheme forces into the setup (marked Required).</summary>
    public string? RequiredVillainGroupId { get; init; }

    /// <summary>A Henchman Group this Scheme forces into the setup (marked Required).</summary>
    public string? RequiredHenchmenGroupId { get; init; }

    /// <summary>Bystanders in the Villain Deck when the Scheme overrides the default.</summary>
    public int? Bystanders { get; init; }

    /// <summary>Short factual setup notes surfaced to the player.</summary>
    public IReadOnlyList<string> Notes { get; init; } = [];
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
