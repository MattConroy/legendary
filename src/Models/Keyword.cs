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

    /// <summary>Concise, flavour-free "remind me how this works" description.</summary>
    public required string Summary { get; init; }

    /// <summary>Full rulebook text, one entry per paragraph/bullet. A "◆" marks a
    /// card symbol (Hero Class / Recruit / Attack icon) that appears inline.</summary>
    public IReadOnlyList<string> Rules { get; init; } = [];

    /// <summary>Ids of the sets this keyword appears in.</summary>
    public IReadOnlyList<string> Sets { get; init; } = [];

    /// <summary>
    /// True when the full <see cref="Rules"/> add meaningful detail beyond the
    /// <see cref="Summary"/> — i.e. worth offering a "Full rules" expander.
    /// </summary>
    public bool HasFullRules =>
        Rules.Count > 0 && string.Join(" ", Rules).Length > Summary.Length + 24;
}
