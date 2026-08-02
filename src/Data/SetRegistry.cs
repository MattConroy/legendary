using Legendary.Companion.Models;

namespace Legendary.Companion.Data;

/// <summary>
/// The single place that lists all known content sets. Adding an expansion is a
/// one-line change here plus a data file — nothing in the randomiser or UI needs
/// to know about specific sets. See Data/_Template.cs for a new-set starter.
///
/// ⚠️ Set ids (and card ids) are permanent: they are the local-storage keys for a
/// player's owned / in-play selections. Never rename or reuse an id once shipped,
/// or saved collections break. Add new sets; don't re-slug existing ones.
/// </summary>
public static class SetRegistry
{
    public static readonly IReadOnlyList<CardSet> AllSets =
    [
        CoreSet.Set,
        DarkCity.Set,
    ];

    public static CardSet? FindById(string id) =>
        AllSets.FirstOrDefault(s => s.Id == id);
}
