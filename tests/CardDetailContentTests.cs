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
        public void Every_base_set_hero_has_a_fourteen_card_deck()
        {
            // The core heroes are the shipped increment; each hero deck is 14 cards.
            foreach (var hero in Set("core").Heroes)
            {
                var deck = CardDetails[hero.Id];
                Assert.Equal(14, deck.Sum(c => c.Copies));
            }
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
        public void Any_class_is_one_of_the_five_hero_classes()
        {
            var classes = new[] { "Covert", "Instinct", "Ranged", "Strength", "Tech" };
            foreach (var deck in CardDetails.Values)
                foreach (var card in deck)
                    if (card.Class is not null)
                        Assert.Contains(card.Class, classes);
        }
    }
}
