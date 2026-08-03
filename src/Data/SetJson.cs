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
        var setIds = new HashSet<string>();
        foreach (var set in sets)
        {
            if (!setIds.Add(set.Id))
                throw new InvalidDataException($"Duplicate set id '{set.Id}'.");
        }

        // Scheme-forced groups may reference other sets (a small box leaning on the
        // Core Skrulls, say), so validate references against every known group.
        var allVillains = sets.SelectMany(s => s.VillainGroups.Select(v => v.Id)).ToHashSet();
        var allHenchmen = sets.SelectMany(s => s.Henchmen.Select(h => h.Id)).ToHashSet();

        foreach (var set in sets)
        {
            foreach (var mm in set.Masterminds)
            {
                if (mm.AlwaysLeadsGroupId is { } g && !allVillains.Contains(g) && !allHenchmen.Contains(g))
                    throw new InvalidDataException($"{set.Id}: Mastermind '{mm.Name}' leads unknown group '{g}'.");
            }

            foreach (var s in set.Schemes)
            {
                foreach (var rv in s.Setup.RequiredVillainGroupIds.Where(rv => !allVillains.Contains(rv)))
                    throw new InvalidDataException($"{set.Id}: Scheme '{s.Name}' requires unknown villain group '{rv}'.");
                foreach (var rh in s.Setup.RequiredHenchmenGroupIds.Where(rh => !allHenchmen.Contains(rh)))
                    throw new InvalidDataException($"{set.Id}: Scheme '{s.Name}' requires unknown henchman group '{rh}'.");
            }
        }
    }
}
