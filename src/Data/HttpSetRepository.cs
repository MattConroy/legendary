using Legendary.Companion.Abstractions;
using Legendary.Companion.Models;

namespace Legendary.Companion.Data;

/// <summary>
/// <see cref="ISetRepository"/> backed by <c>data/sets.json</c>, fetched over HTTP
/// at runtime and cached. Content is data, not code: to change sets you edit the
/// JSON (or, in future, point <see cref="Url"/> at a database/API returning the
/// same shape) — no redeploy of the app is required unless a brand-new team icon
/// is needed.
/// </summary>
public sealed class HttpSetRepository : ISetRepository
{
    private const string Url = "data/sets.json";

    private readonly IHttpClientFactory _httpClientFactory;
    private IReadOnlyList<CardSet>? _sets;

    public HttpSetRepository(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public IReadOnlyList<CardSet> Sets => _sets ?? [];

    public bool IsLoaded => _sets is not null;

    public async Task EnsureLoadedAsync()
    {
        if (_sets is not null) return;
        var http = _httpClientFactory.CreateClient(ContentHttpClient.Name);
        var json = await http.GetStringAsync(Url);
        _sets = SetJson.Deserialize(json);
    }

    public CardSet? FindById(string id) => Sets.FirstOrDefault(s => s.Id == id);
}
