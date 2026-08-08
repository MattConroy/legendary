using Legendary.Companion.Models;
using Xunit;

namespace Legendary.Companion.Tests;

public class SetupTableTests
{
    public class For
    {
        [Theory]
        [InlineData(1, 3, 1, 1, 3, 2)]
        [InlineData(2, 5, 2, 1, 5, 2)]
        [InlineData(3, 5, 3, 1, 5, 8)]
        [InlineData(4, 5, 3, 2, 5, 8)]
        [InlineData(5, 6, 4, 2, 5, 12)]
        public void Matches_the_official_per_player_counts(int players, int heroes, int villains, int henchmen, int strikes, int bystanders)
        {
            var r = SetupTable.For(players);
            Assert.Equal(heroes, r.Heroes);
            Assert.Equal(villains, r.VillainGroups);
            Assert.Equal(henchmen, r.Henchmen);
            Assert.Equal(strikes, r.MasterStrikes);
            Assert.Equal(bystanders, r.Bystanders);
        }
    }
}
