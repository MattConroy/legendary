using Xunit;
using static Legendary.Companion.Tests.Content;

namespace Legendary.Companion.Tests;

/// <summary>
/// Validates the shipped keyword content — <c>keywords.json</c> (the glossary) and
/// <c>card-keywords.json</c> (per-card tags). These assert the data contract, not
/// the <see cref="Legendary.Companion.Models.Keyword"/> class's own behaviour.
/// </summary>
public class KeywordContentTests
{
    public class Glossary
    {
        [Fact]
        public void Loads_with_unique_ids_names_and_summaries()
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
        public void Core_triggers_are_excluded_from_the_ability_keyword_glossary()
        {
            // Scope is ability keywords only — the core triggers aren't listed.
            foreach (var name in new[] { "Ambush", "Fight", "Escape", "Rescue" })
                Assert.DoesNotContain(Keywords, k => k.Name == name);
        }
    }

    public class SetMemberships
    {
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
        public void Every_keyword_set_membership_has_at_least_one_tagged_card()
        {
            // Guards both directions: a keyword claiming a set it never appears on
            // (pollution), and a set whose keyword is never tagged on a card (coverage gap).
            var cardSet = Sets.SelectMany(s => s.AllCards.Select(c => (id: c.Id, set: s.Id)))
                .ToDictionary(t => t.id, t => t.set);

            var tagged = CardKeywords
                .SelectMany(kv => kv.Value.Select(kw => (kw, set: cardSet[kv.Key])))
                .ToHashSet();

            foreach (var k in Keywords)
                foreach (var sid in k.Sets)
                    Assert.True(tagged.Contains((k.Id, sid)),
                        $"Keyword '{k.Name}' lists set '{sid}' but no card there is tagged with it.");
        }
    }

    public class CardTags
    {
        [Fact]
        public void Resolve_to_real_cards_and_keywords()
        {
            var cardIds = Sets.SelectMany(s => s.AllCards.Select(c => c.Id)).ToHashSet();
            var keywordIds = Keywords.Select(k => k.Id).ToHashSet();

            Assert.NotEmpty(CardKeywords);
            foreach (var (cardId, kws) in CardKeywords)
            {
                Assert.Contains(cardId, cardIds);
                Assert.NotEmpty(kws);
                foreach (var kw in kws)
                    Assert.Contains(kw, keywordIds);
            }

            // Sanity: Nightcrawler carries Teleport.
            Assert.Contains("teleport", CardKeywords["dc:nightcrawler"]);

            // Regression: keywords written with a number ("Versatile 3") must still tag.
            Assert.Contains("versatile", CardKeywords["dc:domino"]);
            Assert.Contains("teleport", CardKeywords["dc:cable"]);
        }

        [Fact]
        public void Do_not_mistake_the_scheme_transform_flip_mechanic_for_the_transform_keyword()
        {
            var schemeIds = Sets.SelectMany(s => s.Schemes.Select(c => c.Id)).ToHashSet();
            foreach (var (cardId, kws) in CardKeywords)
                if (schemeIds.Contains(cardId))
                    Assert.DoesNotContain("transform", kws);
        }
    }
}
