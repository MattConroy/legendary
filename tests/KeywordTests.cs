using Legendary.Companion.Data;
using Legendary.Companion.Models;
using Xunit;

namespace Legendary.Companion.Tests;

public class KeywordTests
{
    private static readonly IReadOnlyList<Keyword> Keywords =
        KeywordJson.Deserialize(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "keywords.json")));

    private static readonly IReadOnlyList<CardSet> Sets =
        SetJson.Deserialize(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "sets.json")));

    [Fact]
    public void Glossary_loads_with_unique_ids_and_summaries()
    {
        Assert.NotEmpty(Keywords);
        Assert.Equal(Keywords.Count, Keywords.Select(k => k.Id).Distinct().Count());
        Assert.All(Keywords, k => Assert.False(string.IsNullOrWhiteSpace(k.Summary)));
        Assert.All(Keywords, k => Assert.False(string.IsNullOrWhiteSpace(k.Name)));
    }

    [Fact]
    public void Summaries_are_succinct_and_flavour_free()
    {
        foreach (var k in Keywords)
        {
            // A "remind me" summary stays short and leads with the rule, not lore.
            Assert.True(k.Summary.Length <= 320, $"{k.Name} summary is too long ({k.Summary.Length}).");
            Assert.DoesNotContain("This keyword represents", k.Summary);
        }
    }

    [Fact]
    public void Every_keyword_belongs_to_at_least_one_real_set()
    {
        var setIds = Sets.Select(s => s.Id).ToHashSet();
        foreach (var k in Keywords)
        {
            Assert.NotEmpty(k.Sets);
            foreach (var sid in k.Sets)
                Assert.Contains(sid, setIds);
        }
    }

    [Fact]
    public void Shared_keywords_are_tagged_across_every_set_they_appear_in()
    {
        // Wall-Crawl debuted in Paint the Town Red and recurs in later sets.
        var wallCrawl = Keywords.Single(k => k.Id == "wall-crawl");
        Assert.Contains("paint-the-town-red", wallCrawl.Sets);
        Assert.Contains("secret-wars-vol-2", wallCrawl.Sets);
        Assert.Contains("spider-man-homecoming", wallCrawl.Sets);

        // Teleport is a long-running keyword shared by several boxes.
        Assert.True(Keywords.Single(k => k.Id == "teleport").Sets.Count >= 3);
    }

    [Fact]
    public void Core_triggers_are_excluded_from_the_ability_keyword_glossary()
    {
        // Scope is ability keywords only — the core triggers aren't listed.
        foreach (var name in new[] { "Ambush", "Fight", "Escape", "Rescue" })
            Assert.DoesNotContain(Keywords, k => k.Name == name);
    }

    [Fact]
    public void Card_keyword_tags_resolve_to_real_cards_and_keywords()
    {
        var cardKeywords = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string[]>>(
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "card-keywords.json")), SetJson.Options)!;

        var cardIds = Sets.SelectMany(s =>
            s.Masterminds.Select(c => c.Id)
             .Concat(s.Schemes.Select(c => c.Id))
             .Concat(s.VillainGroups.Select(c => c.Id))
             .Concat(s.Henchmen.Select(c => c.Id))
             .Concat(s.Heroes.Select(c => c.Id))).ToHashSet();
        var keywordIds = Keywords.Select(k => k.Id).ToHashSet();

        Assert.NotEmpty(cardKeywords);
        foreach (var (cardId, kws) in cardKeywords)
        {
            Assert.Contains(cardId, cardIds);
            Assert.NotEmpty(kws);
            foreach (var kw in kws)
                Assert.Contains(kw, keywordIds);
        }

        // Sanity: Nightcrawler carries Teleport.
        Assert.Contains("teleport", cardKeywords["dc:nightcrawler"]);

        // Regression: keywords written with a number ("Versatile 3") must still tag.
        Assert.Contains("versatile", cardKeywords["dc:domino"]);
        Assert.Contains("teleport", cardKeywords["dc:cable"]);

        // Regression: the generic "This Scheme Transforms" flip mechanic must NOT
        // be mistaken for the Transform keyword.
        var schemeIds = Sets.SelectMany(s => s.Schemes.Select(c => c.Id)).ToHashSet();
        foreach (var (cardId, kws) in cardKeywords)
            if (schemeIds.Contains(cardId))
                Assert.DoesNotContain("transform", kws);
    }
}
