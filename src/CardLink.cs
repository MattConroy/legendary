using Legendary.Companion.Models;

namespace Legendary.Companion;

/// <summary>
/// Builds deep-links into the Cards page. Card ids are prefixed with their set id
/// (e.g. "dark-city:bishop"), so a single card links to its set + category + name
/// and lands on exactly that entry; a whole set links with just the set preselected.
/// </summary>
public static class CardLink
{
    /// <summary>Cards page filtered to a single card (its set, category and name).</summary>
    public static string To(GameCard card)
    {
        var query = new Dictionary<string, string?>
        {
            ["category"] = card.Category.ToString(),
            ["set"] = SetIdOf(card.Id),
            ["search"] = card.Name,
        };
        return "cards" + QueryString(query);
    }

    /// <summary>Cards page with a whole set preselected.</summary>
    public static string ToSet(string setId) =>
        "cards" + QueryString(new Dictionary<string, string?> { ["set"] = setId });

    /// <summary>The set a card belongs to, taken from its id prefix.</summary>
    private static string? SetIdOf(string cardId)
    {
        var i = cardId.IndexOf(':');
        return i < 0 ? null : cardId[..i];
    }

    private static string QueryString(Dictionary<string, string?> parameters)
    {
        var parts = parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value!)}");
        var query = string.Join("&", parts);
        return query.Length == 0 ? "" : "?" + query;
    }
}
