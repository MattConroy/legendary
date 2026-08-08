using System.Text.Json;
using Legendary.Companion.Models;

namespace Legendary.Companion.Data;

/// <summary>
/// Deserialises the per-card deck breakdowns from JSON (a map of card id → its
/// cards). Kept separate from transport so the same parsing is used by the app and
/// by tests.
/// </summary>
public static class CardDetailJson
{
    public static IReadOnlyDictionary<string, IReadOnlyList<CardDetail>> Deserialize(string json)
    {
        var map = JsonSerializer.Deserialize<Dictionary<string, List<CardDetail>>>(json, SetJson.Options)
                  ?? throw new InvalidDataException("card-details.json did not contain an object.");
        Validate(map);
        return map.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<CardDetail>)kv.Value);
    }

    /// <summary>Every entry must have cards, and every card a name and a positive copy count.</summary>
    public static void Validate(IReadOnlyDictionary<string, List<CardDetail>> map)
    {
        foreach (var (id, cards) in map)
        {
            if (cards.Count == 0)
                throw new InvalidDataException($"'{id}' has no cards.");
            foreach (var c in cards)
            {
                if (string.IsNullOrWhiteSpace(c.Name))
                    throw new InvalidDataException($"'{id}' has a card with no name.");
                if (c.Copies <= 0)
                    throw new InvalidDataException($"'{id}': card '{c.Name}' has a non-positive copy count.");
            }
        }
    }
}
