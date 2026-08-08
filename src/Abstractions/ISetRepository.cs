using Legendary.Companion.Models;

namespace Legendary.Companion.Abstractions;

/// <summary>
/// Supplies the content sets as domain <see cref="CardSet"/> objects. The
/// application depends on this port, not on where the sets come from — today a
/// JSON file, tomorrow a database or API — so swapping the source only changes
/// the adapter that implements this interface.
/// </summary>
public interface ISetRepository
{
    /// <summary>All known sets, loaded once and cached.</summary>
    IReadOnlyList<CardSet> Sets { get; }

    /// <summary>True once <see cref="EnsureLoadedAsync"/> has completed.</summary>
    bool IsLoaded { get; }

    /// <summary>Load the sets if they aren't loaded already.</summary>
    Task EnsureLoadedAsync();

    /// <summary>The set with this id, or null if none matches.</summary>
    CardSet? FindById(string id);
}
