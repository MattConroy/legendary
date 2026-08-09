using System.Net.Http.Json;
using Legendary.Companion.Abstractions;
using Legendary.Companion.Models;

namespace Legendary.Companion.Data;

/// <summary>
/// <see cref="IKeywordRepository"/> backed by <c>data/keywords.json</c> and
/// <c>data/card-keywords.json</c>, fetched over HTTP at runtime and cached.
/// Mirrors <see cref="HttpSetRepository"/>: content is data, not code.
/// </summary>
public sealed class HttpKeywordRepository : IKeywordRepository
{
    private const string KeywordsUrl = "data/keywords.json";
    private const string CardKeywordsUrl = "data/card-keywords.json";

    private readonly IHttpClientFactory _httpClientFactory;
    private IReadOnlyList<Keyword>? _keywords;
    private IReadOnlyDictionary<string, Keyword> _byId = new Dictionary<string, Keyword>();
    private IReadOnlyDictionary<string, string[]> _cardKeywords = new Dictionary<string, string[]>();

    public HttpKeywordRepository(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public IReadOnlyList<Keyword> Keywords => _keywords ?? [];

    public bool IsLoaded => _keywords is not null;

    public async Task EnsureLoadedAsync()
    {
        if (_keywords is not null) return;
        var http = _httpClientFactory.CreateClient(ContentHttpClient.Name);
        var keywords = KeywordJson.Deserialize(await http.GetStringAsync(KeywordsUrl));

        // The per-card tags are a nice-to-have; if they fail to load, the glossary
        // still works and "keywords in play" just stays empty.
        Dictionary<string, string[]> cards;
        try
        {
            cards = await http.GetFromJsonAsync<Dictionary<string, string[]>>(CardKeywordsUrl, SetJson.Options)
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
