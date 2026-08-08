using Legendary.Companion.Models;
using Xunit;

namespace Legendary.Companion.Tests;

public class SchemeSetupTests
{
    public class HeroesFor
    {
        [Fact]
        public void Falls_back_to_the_base_table_plus_the_delta()
            => Assert.Equal(SetupTable.For(2).Heroes + 1,
                new SchemeSetup { HeroDelta = 1 }.HeroesFor(SetupTable.For(2), 2));

        [Fact]
        public void An_absolute_override_beats_the_table()
            => Assert.Equal(6, new SchemeSetup { Heroes = 6 }.HeroesFor(SetupTable.For(2), 2));

        [Fact]
        public void A_per_player_override_beats_an_absolute_one()
        {
            var s = new SchemeSetup { Heroes = 6, HeroDelta = 3, HeroesByPlayers = new Dictionary<int, int> { [2] = 4 } };
            Assert.Equal(4, s.HeroesFor(SetupTable.For(2), 2)); // 2p listed -> 4
            Assert.Equal(6, s.HeroesFor(SetupTable.For(3), 3)); // 3p not listed -> absolute 6
        }
    }

    public class VillainGroupsFor
    {
        [Fact]
        public void Adds_the_delta_to_the_base_table()
            => Assert.Equal(SetupTable.For(2).VillainGroups + 1,
                new SchemeSetup { VillainGroupDelta = 1 }.VillainGroupsFor(SetupTable.For(2)));
    }

    public class HenchmenFor
    {
        [Fact]
        public void Adds_the_delta_to_the_base_table()
            => Assert.Equal(SetupTable.For(2).Henchmen + 1,
                new SchemeSetup { HenchmenDelta = 1 }.HenchmenFor(SetupTable.For(2)));
    }

    public class TwistsFor
    {
        [Fact]
        public void Defaults_to_the_flat_twist_count()
            => Assert.Equal(8, new SchemeSetup().TwistsFor(3));

        [Fact]
        public void Uses_a_per_player_override_when_present()
        {
            var s = new SchemeSetup { Twists = 8, TwistsByPlayers = new Dictionary<int, int> { [2] = 7 } };
            Assert.Equal(7, s.TwistsFor(2)); // 2p listed
            Assert.Equal(8, s.TwistsFor(5)); // 5p not listed -> flat count
        }
    }

    public class BystandersFor
    {
        [Fact]
        public void Defaults_to_the_base_rule()
            => Assert.Equal(SetupTable.For(3).Bystanders, new SchemeSetup().BystandersFor(SetupTable.For(3)));

        [Fact]
        public void Uses_the_scheme_override_when_set()
            => Assert.Equal(0, new SchemeSetup { Bystanders = 0 }.BystandersFor(SetupTable.For(3)));
    }
}
