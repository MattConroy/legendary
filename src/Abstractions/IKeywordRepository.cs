using Legendary.Companion.Models;

namespace Legendary.Companion.Abstractions;

/// <summary>
/// Supplies the keyword glossary and per-card keyword tags as domain
/// <see cref="Keyword"/> objects. Like <see cref="ISetRepository"/>, the
/// application depends on this port rather than on the JSON that backs it today.
/// </summary>
public interface IKeywordRepository
{
    /// <summary>All keywords, loaded once and cached.</summary>
    IReadOnlyList<Keyword> Keywords { get; }

    /// <summary>True once <see cref="EnsureLoadedAsync"/> has completed.</summary>
    bool IsLoaded { get; }

    /// <summary>Load the glossary and per-card tags if they aren't loaded already.</summary>
    Task EnsureLoadedAsync();

    /// <summary>The keyword with this id, or null if none matches.</summary>
    Keyword? ById(string id);

    /// <summary>The keywords appearing on the given cards, de-duplicated and ordered by name.</summary>
    IReadOnlyList<Keyword> InPlay(IEnumerable<string> cardIds);
}
