using Legendary.Companion.Models;

namespace Legendary.Companion.Data;

/// <summary>
/// The single place that lists all known content sets. Adding an expansion is a
/// one-line change here plus a data file — nothing in the randomiser or UI needs
/// to know about specific sets.
/// </summary>
public static class SetRegistry
{
    public static readonly IReadOnlyList<CardSet> AllSets =
    [
        CoreSet.Set,
        ExampleExpansion.Set,
    ];

    public static CardSet? FindById(string id) =>
        AllSets.FirstOrDefault(s => s.Id == id);
}
