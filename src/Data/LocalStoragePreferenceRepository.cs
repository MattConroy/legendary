using System.Text.Json;
using Legendary.Companion.Abstractions;
using Microsoft.JSInterop;

namespace Legendary.Companion.Data;

/// <summary>
/// <see cref="IPreferenceRepository"/> backed by the browser's local storage via
/// JS interop. Lists are stored as JSON. Every call is wrapped so a storage
/// failure (e.g. private-browsing mode) degrades to "unset"/no-op rather than
/// throwing into the application layer.
/// </summary>
public sealed class LocalStoragePreferenceRepository : IPreferenceRepository
{
    private readonly IJSRuntime _js;

    public LocalStoragePreferenceRepository(IJSRuntime js) => _js = js;

    public async Task<string?> GetAsync(string key)
    {
        try { return await _js.InvokeAsync<string?>("localStorage.getItem", key); }
        catch { return null; }
    }

    public async Task SetAsync(string key, string value)
    {
        try { await _js.InvokeVoidAsync("localStorage.setItem", key, value); }
        catch { /* ignore storage failures */ }
    }

    public async Task<IReadOnlyList<string>?> GetListAsync(string key)
    {
        var json = await GetAsync(key);
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonSerializer.Deserialize<List<string>>(json); }
        catch { return null; }
    }

    public Task SetListAsync(string key, IEnumerable<string> values) =>
        SetAsync(key, JsonSerializer.Serialize(values.ToList()));
}
