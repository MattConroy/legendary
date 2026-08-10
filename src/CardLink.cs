using Legendary.Companion.Models;

namespace Legendary.Companion;

/// <summary>
/// Builds deep-links into the Cards page. A card links by its category and name,
/// which is enough to land on it; callers that know the card's set (e.g. the Sets
/// list) can pass it to also preselect the set filter. Card-id prefixes are *not* a
/// reliable set id (e.g. "dc:" belongs to "dark-city"), so we never derive it.
/// </summary>
public static class CardLink
{
    /// <summary>Cards page filtered to a card by category and name, optionally within a set.</summary>
    public static string To(GameCard card, string? setId = null)
    {
        var query = new Dictionary<string, string?>
        {
            ["category"] = card.Category.ToString(),
            ["set"] = setId,
            ["search"] = card.Name,
        };
        return "cards" + QueryString(query);
    }

    /// <summary>Cards page with a whole set preselected.</summary>
    public static string ToSet(string setId) =>
        "cards" + QueryString(new Dictionary<string, string?> { ["set"] = setId });

    private static string QueryString(Dictionary<string, string?> parameters)
    {
        var parts = parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value!)}");
        var query = string.Join("&", parts);
        return query.Length == 0 ? "" : "?" + query;
    }
}
