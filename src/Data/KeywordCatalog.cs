using Legendary.Companion.Models;

namespace Legendary.Companion.Data;

/// <summary>
/// Loads the keyword glossary at runtime from <c>data/keywords.json</c> and caches
/// it. Mirrors <see cref="SetCatalog"/>: content is data, not code.
/// </summary>
public sealed class KeywordCatalog
{
    private const string Url = "data/keywords.json";

    private readonly HttpClient _http;
    private IReadOnlyList<Keyword>? _keywords;

    public KeywordCatalog(HttpClient http) => _http = http;

    /// <summary>All keywords, loaded once and cached.</summary>
    public IReadOnlyList<Keyword> Keywords => _keywords ?? [];

    public bool IsLoaded => _keywords is not null;

    public async Task EnsureLoadedAsync()
    {
        if (_keywords is not null) return;
        var json = await _http.GetStringAsync(Url);
        _keywords = KeywordJson.Deserialize(json);
    }
}
