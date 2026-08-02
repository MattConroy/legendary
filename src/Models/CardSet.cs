namespace Legendary.Companion.Models;

/// <summary>
/// A self-contained content module — the Core Set or a future expansion.
/// The randomiser only ever sees the aggregate pool of cards from *enabled*
/// sets, so a new expansion is added by dropping in one <see cref="CardSet"/>
/// and registering it. No randomiser code changes.
/// </summary>
public sealed record CardSet
{
    public required string Id { get; init; }
    public required string Name { get; init; }

    /// <summary>Short description shown in the Settings toggle list.</summary>
    public string? Description { get; init; }

    /// <summary>
    /// Whether this set is toggled on by default. Users can override this in
    /// Settings; their choice is persisted to local storage.
    /// </summary>
    public bool EnabledByDefault { get; init; } = true;

    /// <summary>
    /// True for illustrative/sample content that is not from an official product.
    /// Surfaced clearly in the UI so it is never mistaken for real cards.
    /// </summary>
    public bool IsExample { get; init; }

    public IReadOnlyList<Mastermind> Masterminds { get; init; } = [];
    public IReadOnlyList<Scheme> Schemes { get; init; } = [];
    public IReadOnlyList<VillainGroup> VillainGroups { get; init; } = [];
    public IReadOnlyList<Henchmen> Henchmen { get; init; } = [];
    public IReadOnlyList<Hero> Heroes { get; init; } = [];
}
