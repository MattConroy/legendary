using Xunit;
using static Legendary.Companion.Tests.Content;

namespace Legendary.Companion.Tests;

/// <summary>
/// Validates the shipped <c>card-details.json</c> (per-card deck breakdowns): every
/// entry must point at a real card, and known decks must have the expected shape.
/// Facts only — no ability text is modelled or asserted.
/// </summary>
public class CardDetailContentTests
{
    public class Keys
    {
        [Fact]
        public void Every_entry_resolves_to_a_real_card()
        {
            var cardIds = Sets.SelectMany(s => s.AllCards.Select(c => c.Id)).ToHashSet();
            foreach (var id in CardDetails.Keys)
                Assert.Contains(id, cardIds);
        }
    }

    public class Decks
    {
        [Fact]
        public void Every_hero_deck_totals_fourteen_cards()
        {
            // Every Legendary hero deck is 14 cards (transformed flip-sides excluded).
            foreach (var (id, deck) in CardDetails)
                Assert.Equal(14, deck.Sum(c => c.Copies));
        }

        [Fact]
        public void Covers_every_hero_in_the_catalogue()
        {
            var heroIds = Sets.SelectMany(s => s.Heroes.Select(h => h.Id)).ToHashSet();
            Assert.Equal(heroIds.Count, CardDetails.Count);
            foreach (var id in heroIds)
                Assert.Contains(id, CardDetails.Keys);
        }

        [Fact]
        public void Cards_that_declare_a_value_declare_a_resource_kind()
        {
            foreach (var deck in CardDetails.Values)
                foreach (var card in deck)
                    if (card.Value is not null)
                        Assert.Contains(card.Kind, new[] { "attack", "recruit" });
        }

        [Fact]
        public void Every_class_is_one_of_the_five_hero_classes()
        {
            var classes = new[] { "Covert", "Instinct", "Ranged", "Strength", "Tech" };
            foreach (var deck in CardDetails.Values)
                foreach (var card in deck)
                    foreach (var cls in card.Classes)
                        Assert.Contains(cls, classes);
        }

        [Fact]
        public void No_card_declares_more_than_two_classes()
        {
            foreach (var deck in CardDetails.Values)
                foreach (var card in deck)
                    Assert.True(card.Classes.Count <= 2, $"{card.Name} has {card.Classes.Count} classes");
        }
    }
}
