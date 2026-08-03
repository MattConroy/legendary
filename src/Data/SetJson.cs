using System.Text.Json;
using Legendary.Companion.Models;

namespace Legendary.Companion.Data;

/// <summary>
/// Deserialises the content sets from JSON. Kept separate from any transport so
/// the same parsing is used by the app (fetching <c>data/sets.json</c>) and by
/// tests (reading the file from disk). Swapping JSON for a database/API later only
/// changes where the string comes from, not this parsing.
/// </summary>
public static class SetJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static IReadOnlyList<CardSet> Deserialize(string json)
    {
        var sets = JsonSerializer.Deserialize<List<CardSet>>(json, Options)
                   ?? throw new InvalidDataException("sets.json did not contain a set array.");
        Validate(sets);
        return sets;
    }

    /// <summary>
    /// Runtime equivalent of the old compile-time safety: every Mastermind's
    /// "Always Leads" id and every Scheme-forced group id must resolve to a real
    /// group, and set/card ids must be unique. Throws on bad data.
    /// </summary>
    public static void Validate(IReadOnlyList<CardSet> sets)
    {
        var ids = new HashSet<string>();
        foreach (var set in sets)
        {
            if (!ids.Add(set.Id))
                throw new InvalidDataException($"Duplicate set id '{set.Id}'.");

            var groups = set.VillainGroups.Select(v => v.Id)
                .Concat(set.Henchmen.Select(h => h.Id))
                .ToHashSet();

            foreach (var mm in set.Masterminds)
            {
                if (mm.AlwaysLeadsGroupId is { } g && !groups.Contains(g))
                    throw new InvalidDataException($"{set.Id}: Mastermind '{mm.Name}' leads unknown group '{g}'.");
            }

            foreach (var s in set.Schemes)
            {
                if (s.Setup.RequiredVillainGroupId is { } rv && set.VillainGroups.All(v => v.Id != rv))
                    throw new InvalidDataException($"{set.Id}: Scheme '{s.Name}' requires unknown villain group '{rv}'.");
                if (s.Setup.RequiredHenchmenGroupId is { } rh && set.Henchmen.All(h => h.Id != rh))
                    throw new InvalidDataException($"{set.Id}: Scheme '{s.Name}' requires unknown henchman group '{rh}'.");
            }
        }
    }
}
