using System.Text.Json;
using Legendary.Companion.Models;

namespace Legendary.Companion.Data;

/// <summary>
/// Deserialises the keyword glossary from JSON. Kept separate from transport so the
/// same parsing is used by the app (fetching <c>data/keywords.json</c>) and by tests.
/// </summary>
public static class KeywordJson
{
    public static IReadOnlyList<Keyword> Deserialize(string json)
    {
        var keywords = JsonSerializer.Deserialize<List<Keyword>>(json, SetJson.Options)
                       ?? throw new InvalidDataException("keywords.json did not contain a keyword array.");
        Validate(keywords);
        return keywords;
    }

    /// <summary>Ids must be unique and every keyword must carry a definition.</summary>
    public static void Validate(IReadOnlyList<Keyword> keywords)
    {
        var ids = new HashSet<string>();
        foreach (var k in keywords)
        {
            if (!ids.Add(k.Id))
                throw new InvalidDataException($"Duplicate keyword id '{k.Id}'.");
            if (string.IsNullOrWhiteSpace(k.Summary))
                throw new InvalidDataException($"Keyword '{k.Name}' has no summary.");
        }
    }
}
