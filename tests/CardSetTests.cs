using Legendary.Companion.Models;
using Xunit;
using static Legendary.Companion.Tests.Content;

namespace Legendary.Companion.Tests;

public class CardSetTests
{
    public class AllCards
    {
        [Fact]
        public void Includes_every_category_of_card()
        {
            var set = Set("core");
            var expected = set.Masterminds.Count + set.Schemes.Count + set.VillainGroups.Count
                         + set.Henchmen.Count + set.Heroes.Count;
            Assert.Equal(expected, set.AllCards.Count());
            Assert.Contains(set.AllCards, c => c is Mastermind);
            Assert.Contains(set.AllCards, c => c is Scheme);
            Assert.Contains(set.AllCards, c => c is Hero);
        }
    }
}
