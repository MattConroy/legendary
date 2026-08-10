using Legendary.Companion.Models;
using Legendary.Companion.Services;
using Xunit;
using static Legendary.Companion.Tests.Content;

namespace Legendary.Companion.Tests;

public class CardPoolTests
{
    public class From
    {
        [Fact]
        public void Aggregates_cards_from_every_given_set()
            => Assert.True(AllPool().Heroes.Count > CorePool().Heroes.Count);

        [Fact]
        public void Excludes_cards_from_sets_outside_the_pool()
        {
            // A Dark City card is present in the full pool but not in a Core-only pool.
            Assert.Contains(AllPool().Heroes, h => h.Id == "dark-city:bishop");
            Assert.DoesNotContain(CorePool().Heroes, h => h.Id == "dark-city:bishop");
        }

        [Fact]
        public void FindById_resolves_a_card_across_categories_or_returns_null()
        {
            var pool = CorePool();
            Assert.IsType<Mastermind>(pool.FindById("core:dr-doom"));
            Assert.Null(pool.FindById("does-not-exist"));
            Assert.Null(pool.FindById(null));
        }
    }

    public class IsPlayable
    {
        [Fact]
        public void Is_true_when_every_category_has_a_card()
            => Assert.True(CorePool().IsPlayable);

        [Fact]
        public void Is_false_when_a_category_is_empty()
            => Assert.False(CardPool.From([Set("dimensions")]).IsPlayable); // a mix-in: no schemes/villains alone
    }
}
