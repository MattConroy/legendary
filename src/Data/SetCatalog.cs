using System.Net.Http.Json;
using Legendary.Companion.Models;

namespace Legendary.Companion.Data;

/// <summary>
/// Loads the content sets at runtime from <c>data/sets.json</c> and caches them.
/// Content is data, not code: to change sets you edit the JSON (or, in future,
/// point <see cref="Url"/> at a database/API returning the same shape) — no
/// redeploy of the app is required unless a brand-new team icon is needed.
/// </summary>
public sealed class SetCatalog
{
    private const string Url = "data/sets.json";

    private readonly HttpClient _http;
    private IReadOnlyList<CardSet>? _sets;

    public SetCatalog(HttpClient http) => _http = http;

    /// <summary>All known sets, loaded once and cached.</summary>
    public IReadOnlyList<CardSet> Sets => _sets ?? [];

    public bool IsLoaded => _sets is not null;

    public async Task EnsureLoadedAsync()
    {
        if (_sets is not null) return;
        var json = await _http.GetStringAsync(Url);
        _sets = SetJson.Deserialize(json);
    }

    public CardSet? FindById(string id) => Sets.FirstOrDefault(s => s.Id == id);
}
