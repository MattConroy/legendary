using Legendary.Companion.Abstractions;
using Legendary.Companion.Models;

namespace Legendary.Companion.Data;

/// <summary>
/// <see cref="ICardDetailRepository"/> backed by <c>data/card-details.json</c>,
/// fetched over HTTP at runtime and cached. Mirrors the other repositories:
/// content is data, not code.
/// </summary>
public sealed class HttpCardDetailRepository : ICardDetailRepository
{
    private const string Url = "data/card-details.json";

    private readonly HttpClient _http;
    private IReadOnlyDictionary<string, IReadOnlyList<CardDetail>>? _byId;

    public HttpCardDetailRepository(HttpClient http) => _http = http;

    public bool IsLoaded => _byId is not null;

    public async Task EnsureLoadedAsync()
    {
        if (_byId is not null) return;
        _byId = CardDetailJson.Deserialize(await _http.GetStringAsync(Url));
    }

    public IReadOnlyList<CardDetail> For(string cardId) =>
        _byId is not null && _byId.TryGetValue(cardId, out var cards) ? cards : [];
}
