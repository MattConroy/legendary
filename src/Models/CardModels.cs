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

    /// <summary>Optional flavour tagline.</summary>
    public string? Tagline { get; init; }
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
/// Adjustments a Scheme makes to the base per-player setup, plus any human-readable
/// setup notes ("Add extra Bystanders", etc.) that don't map to a randomised category.
/// </summary>
public sealed record SchemeSetup
{
    public static readonly SchemeSetup None = new();

    public int HeroDelta { get; init; }
    public int VillainGroupDelta { get; init; }
    public int HenchmenDelta { get; init; }

    /// <summary>Notes surfaced to the player (extra twists, bystanders, strikes, etc.).</summary>
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
