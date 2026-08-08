using System.Text.Json;
using Legendary.Companion.Data;
using Legendary.Companion.Models;
using Legendary.Companion.Services;
using Xunit;

namespace Legendary.Companion.Tests;

/// <summary>
/// Shared access to the shipped content, loaded once from the JSON copied next to
/// the test binary. Used by the content-validation suites and by unit tests that
/// need a realistic card pool.
/// </summary>
internal static class Content
{
    public static readonly IReadOnlyList<CardSet> Sets =
        SetJson.Deserialize(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "sets.json")));

    public static readonly IReadOnlyList<Keyword> Keywords =
        KeywordJson.Deserialize(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "keywords.json")));

    public static readonly IReadOnlyDictionary<string, string[]> CardKeywords =
        JsonSerializer.Deserialize<Dictionary<string, string[]>>(
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "card-keywords.json")), SetJson.Options)!;

    public static readonly IReadOnlyDictionary<string, IReadOnlyList<CardDetail>> CardDetails =
        CardDetailJson.Deserialize(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "card-details.json")));

    public static CardSet Set(string id) => Sets.First(s => s.Id == id);

    public static CardPool CorePool() => CardPool.From([Set("core")]);

    public static CardPool AllPool() => CardPool.From(Sets);
}

/// <summary>Helpers for driving the randomiser deterministically in tests.</summary>
internal static class Roll
{
    /// <summary>A randomiser seeded so assertions are stable.</summary>
    public static SetupRandomizer Rng(int seed = 12345) => new(new Random(seed));

    /// <summary>Generate, then reroll the Scheme until a specific one is drawn.</summary>
    public static GameSetup WithScheme(SetupRandomizer r, CardPool pool, int players, string schemeId)
    {
        var s = r.Generate(players, pool);
        for (var i = 0; i < 3000 && s.Scheme.Card.Id != schemeId; i++)
            s = r.Reroll(s, CardCategory.Scheme, pool);
        Assert.Equal(schemeId, s.Scheme.Card.Id);
        return s;
    }
}
