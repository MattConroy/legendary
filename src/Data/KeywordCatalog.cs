using System.Net.Http.Json;
using System.Text.Json;
using Legendary.Companion.Models;

namespace Legendary.Companion.Data;

/// <summary>
/// Loads the keyword glossary and the per-card keyword tags at runtime and caches
/// them. Mirrors <see cref="SetCatalog"/>: content is data, not code.
/// </summary>
public sealed class KeywordCatalog
{
    private const string KeywordsUrl = "data/keywords.json";
    private const string CardKeywordsUrl = "data/card-keywords.json";

    private readonly HttpClient _http;
    private IReadOnlyList<Keyword>? _keywords;
    private IReadOnlyDictionary<string, Keyword> _byId = new Dictionary<string, Keyword>();
    private IReadOnlyDictionary<string, string[]> _cardKeywords = new Dictionary<string, string[]>();

    public KeywordCatalog(HttpClient http) => _http = http;

    /// <summary>All keywords, loaded once and cached.</summary>
    public IReadOnlyList<Keyword> Keywords => _keywords ?? [];

    public bool IsLoaded => _keywords is not null;

    public async Task EnsureLoadedAsync()
    {
        if (_keywords is not null) return;
        var keywords = KeywordJson.Deserialize(await _http.GetStringAsync(KeywordsUrl));

        // The per-card tags are a nice-to-have; if they fail to load, the glossary
        // still works and "keywords in play" just stays empty.
        Dictionary<string, string[]> cards;
        try
        {
            cards = await _http.GetFromJsonAsync<Dictionary<string, string[]>>(CardKeywordsUrl, SetJson.Options)
                    ?? new Dictionary<string, string[]>();
        }
        catch
        {
            cards = new Dictionary<string, string[]>();
        }

        _keywords = keywords;
        _byId = keywords.ToDictionary(k => k.Id);
        _cardKeywords = cards;
    }

    public Keyword? ById(string id) => _byId.GetValueOrDefault(id);

    /// <summary>The keywords appearing on the given cards, de-duplicated and ordered by name.</summary>
    public IReadOnlyList<Keyword> InPlay(IEnumerable<string> cardIds)
    {
        var ids = new HashSet<string>();
        foreach (var cardId in cardIds)
            if (_cardKeywords.TryGetValue(cardId, out var kws))
                foreach (var k in kws) ids.Add(k);

        return ids.Select(ById).OfType<Keyword>()
                  .OrderBy(k => k.Name, StringComparer.OrdinalIgnoreCase)
                  .ToList();
    }
}
