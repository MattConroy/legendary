using Legendary.Companion.Models;
using Xunit;
using static Legendary.Companion.Tests.Content;
using static Legendary.Companion.Tests.Roll;

namespace Legendary.Companion.Tests;

public class GameSetupTests
{
    public class Threat
    {
        [Fact]
        public void Combines_the_card_contributions_and_stays_in_range()
        {
            for (var i = 0; i < 50; i++)
            {
                var setup = Rng(i).Generate((i % 5) + 1, AllPool());
                var mm = (Mastermind)setup.Mastermind.Card;
                var sc = (Scheme)setup.Scheme.Card;
                Assert.Equal(Models.Threat.From(mm.ThreatBase!.Value, sc.ThreatModifier), setup.Threat!.Value);
                Assert.InRange(setup.Threat!.Value.Score, 1, 10);
            }
        }
    }

    public class AllCards
    {
        [Fact]
        public void Lists_the_mastermind_scheme_and_every_group_and_hero()
        {
            var setup = Rng(1).Generate(3, AllPool());
            var expected = 2 + setup.VillainGroups.Count + setup.Henchmen.Count + setup.Heroes.Count;
            Assert.Equal(expected, setup.AllCards.Count());
            Assert.Contains(setup.Mastermind.Card, setup.AllCards);
            Assert.Contains(setup.Scheme.Card, setup.AllCards);
        }

        [Fact]
        public void AllCardIds_are_the_ids_of_AllCards()
        {
            var setup = Rng(2).Generate(4, AllPool());
            Assert.Equal(setup.AllCards.Select(c => c.Id), setup.AllCardIds);
        }
    }
}
