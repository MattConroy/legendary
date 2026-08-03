namespace Legendary.Companion.Models;

/// <summary>
/// A game keyword (ability word) and its rulebook definition. Keywords are shared
/// across expansions, so a keyword lists every set it appears in rather than being
/// duplicated per set. Content is data — see <c>wwwroot/data/keywords.json</c>.
/// </summary>
public sealed record Keyword
{
    /// <summary>Stable slug id, unique across all keywords (e.g. "wall-crawl").</summary>
    public required string Id { get; init; }

    /// <summary>Display name (e.g. "Wall-Crawl").</summary>
    public required string Name { get; init; }

    /// <summary>Concise rulebook definition.</summary>
    public required string Definition { get; init; }

    /// <summary>Ids of the sets this keyword appears in.</summary>
    public IReadOnlyList<string> Sets { get; init; } = [];
}
