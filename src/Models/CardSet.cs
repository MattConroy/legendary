namespace Legendary.Companion.Models;

/// <summary>
/// A self-contained content module — a Core set or an expansion. The randomiser
/// only ever sees the aggregate pool of cards from *owned and enabled* sets, so a
/// new set is added by dropping in one <see cref="CardSet"/> and registering it.
/// No randomiser code changes.
/// </summary>
public sealed record CardSet
{
    public required string Id { get; init; }
    public required string Name { get; init; }

    /// <summary>Short description (not shown in the streamlined list, kept for tooltips/future use).</summary>
    public string? Description { get; init; }

    /// <summary>Release date, used for display and sorting.</summary>
    public required DateOnly Released { get; init; }

    /// <summary>
    /// True for a "big box" that can be played on its own (Core Set, What If?,
    /// Second Edition…). Expansions that need a Core set are false.
    /// </summary>
    public bool Standalone { get; init; }

    /// <summary>
    /// Whether the set is assumed owned + enabled the first time the app runs.
    /// Users override both in the Sets page; choices persist to local storage.
    /// </summary>
    public bool EnabledByDefault { get; init; } = true;

    public IReadOnlyList<Mastermind> Masterminds { get; init; } = [];
    public IReadOnlyList<Scheme> Schemes { get; init; } = [];
    public IReadOnlyList<VillainGroup> VillainGroups { get; init; } = [];
    public IReadOnlyList<Henchmen> Henchmen { get; init; } = [];
    public IReadOnlyList<Hero> Heroes { get; init; } = [];

    /// <summary>Every card in this set, across all categories.</summary>
    public IEnumerable<GameCard> AllCards =>
        Masterminds.Cast<GameCard>()
            .Concat(Schemes)
            .Concat(VillainGroups)
            .Concat(Henchmen)
            .Concat(Heroes);
}
