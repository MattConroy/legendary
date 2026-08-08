namespace Legendary.Companion.Abstractions;

/// <summary>
/// Persists user preferences by key. Hides both the transport (browser local
/// storage) and the encoding, so the application layer reads and writes plain
/// values and lists without touching JS interop or JSON. All operations are
/// best-effort: when storage is unavailable, reads return "unset" and writes
/// are silently dropped.
/// </summary>
public interface IPreferenceRepository
{
    /// <summary>The stored value for a key, or null if unset.</summary>
    Task<string?> GetAsync(string key);

    /// <summary>Store a scalar value.</summary>
    Task SetAsync(string key, string value);

    /// <summary>The stored list for a key, or null if the key was never written.</summary>
    Task<IReadOnlyList<string>?> GetListAsync(string key);

    /// <summary>Store a list of values.</summary>
    Task SetListAsync(string key, IEnumerable<string> values);
}
