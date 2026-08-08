using Legendary.Companion.Models;

namespace Legendary.Companion.Abstractions;

/// <summary>
/// Supplies per-card deck breakdowns (factual stats) for the entities that have
/// them, as domain <see cref="CardDetail"/> objects. Like the other repositories,
/// the app depends on this port rather than on the JSON that backs it today.
/// </summary>
public interface ICardDetailRepository
{
    /// <summary>True once <see cref="EnsureLoadedAsync"/> has completed.</summary>
    bool IsLoaded { get; }

    /// <summary>Load the breakdowns if they aren't loaded already.</summary>
    Task EnsureLoadedAsync();

    /// <summary>The cards making up the given entity's deck, or empty if none are known.</summary>
    IReadOnlyList<CardDetail> For(string cardId);
}
