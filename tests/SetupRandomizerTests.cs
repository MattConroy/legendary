using Legendary.Companion.Models;
using Legendary.Companion.Services;
using Xunit;
using static Legendary.Companion.Tests.Content;
using static Legendary.Companion.Tests.Roll;

namespace Legendary.Companion.Tests;

public class SetupRandomizerTests
{
    public class Generate
    {
        [Fact]
        public void Effective_counts_match_the_selected_card_counts()
        {
            var pool = AllPool();
            for (var i = 0; i < 300; i++)
            {
                var setup = Rng(i).Generate((i % 5) + 1, pool);
                Assert.Equal(setup.EffectiveHeroCount, setup.Heroes.Count);
                Assert.Equal(setup.EffectiveVillainGroupCount, setup.VillainGroups.Count);
                Assert.Equal(setup.EffectiveHenchmenCount, setup.Henchmen.Count);
            }
        }

        [Fact]
        public void Produces_distinct_cards_within_each_category()
        {
            var setup = Rng().Generate(5, AllPool());
            AssertDistinct(setup.VillainGroups);
            AssertDistinct(setup.Henchmen);
            AssertDistinct(setup.Heroes);
        }

        [Fact]
        public void Always_includes_the_masterminds_always_leads_group_marked_required()
        {
            var pool = CorePool();
            for (var i = 0; i < 500; i++)
            {
                var setup = Rng(i).Generate(3, pool);
                var mm = (Mastermind)setup.Mastermind.Card;

                // The Mastermind's Always-Leads group must be present and flagged Required
                // (a Scheme may add further required groups, so don't assume exactly one).
                var alwaysLeads = setup.VillainGroups.Concat(setup.Henchmen)
                    .FirstOrDefault(s => s.Card.Id == mm.AlwaysLeadsGroupId);

                Assert.NotNull(alwaysLeads);
                Assert.True(alwaysLeads!.IsRequired);
            }
        }

        [Fact]
        public void Scheme_setup_deltas_change_the_effective_counts()
        {
            // Uses a synthetic set so the mechanism is tested without asserting any
            // real-game ruling (shipped Schemes currently declare no deltas).
            const string s = "t";
            var set = new CardSet
            {
                Id = s, Name = "Test", Released = new DateOnly(2020, 1, 1),
                Masterminds = [new() { Id = "t:m", Name = "M", AlwaysLeadsGroupId = "t:v1" }],
                Schemes = [new() { Id = "t:s", Name = "S", Setup = new SchemeSetup { HenchmenDelta = 1, HeroDelta = 1 } }],
                VillainGroups = [new() { Id = "t:v1", Name = "V1" }, new() { Id = "t:v2", Name = "V2" }, new() { Id = "t:v3", Name = "V3" }],
                Henchmen = [new() { Id = "t:h1", Name = "H1" }, new() { Id = "t:h2", Name = "H2" }, new() { Id = "t:h3", Name = "H3" }],
                Heroes = Enumerable.Range(1, 8).Select(i => new Hero { Id = $"t:hero{i}", Name = $"Hero {i}" }).ToList(),
            };
            var pool = CardPool.From([set]);

            var setup = new SetupRandomizer(new Random(1)).Generate(2, pool); // 2p base: 5 heroes, 1 henchman
            Assert.Equal(6, setup.Heroes.Count);        // 5 + HeroDelta 1
            Assert.Equal(2, setup.Henchmen.Count);      // 1 + HenchmenDelta 1
            Assert.Equal(6, setup.EffectiveHeroCount);
            Assert.Equal(2, setup.EffectiveHenchmenCount);
        }

        [Theory]
        [InlineData("core:dark-dimension", 7)]     // Portals to the Dark Dimension
        [InlineData("core:killbots", 5)]           // Replace Earth's Leaders with Killbots
        [InlineData("core:legacy-virus", 8)]
        public void Applies_exact_scheme_twist_counts(string schemeId, int twists)
            => Assert.Equal(twists, WithScheme(Rng(1), CorePool(), 3, schemeId).EffectiveTwists);

        [Fact]
        public void Killbots_does_not_add_a_henchman_group_and_uses_18_bystanders()
        {
            var setup = WithScheme(Rng(4), CorePool(), 2, "core:killbots");
            Assert.Single(setup.Henchmen);               // 2p base henchmen, unchanged
            Assert.Equal(18, setup.EffectiveBystanders);
        }

        [Fact]
        public void Negative_zone_adds_an_extra_henchman_group()
        {
            var setup = WithScheme(Rng(5), CorePool(), 2, "core:negative-zone");
            Assert.Equal(2, setup.EffectiveHenchmenCount); // 1 base + 1
            Assert.Equal(2, setup.Henchmen.Count);
        }

        [Fact]
        public void Steal_the_plutonium_adds_an_extra_villain_group()
        {
            var pool = CardPool.From([Set("core"), Set("dark-city")]);
            var setup = WithScheme(Rng(6), pool, 2, "dc:steal-plutonium");
            Assert.Equal(3, setup.EffectiveVillainGroupCount); // 2 base + 1
            Assert.Equal(3, setup.VillainGroups.Count);
        }

        [Fact]
        public void Secret_invasion_uses_six_heroes_and_forces_skrulls()
        {
            var setup = WithScheme(Rng(7), CorePool(), 2, "core:secret-invasion");
            Assert.Equal(6, setup.EffectiveHeroCount);
            Assert.Equal(6, setup.Heroes.Count);
            Assert.Contains(setup.VillainGroups, v => v.Card.Id == "core:skrulls" && v.IsRequired);
        }

        [Theory]
        [InlineData(2, 4, 8)]   // 2 players: 4 heroes, 8 twists
        [InlineData(3, 5, 8)]   // 3 players: 5 heroes, 8 twists
        [InlineData(4, 5, 5)]   // 4 players: 5 heroes, 5 twists
        [InlineData(5, 6, 5)]   // 5 players: 6 heroes, 5 twists
        public void Civil_war_scales_heroes_and_twists_by_player_count(int players, int heroes, int twists)
        {
            var setup = WithScheme(Rng(8), CorePool(), players, "core:civil-war");
            Assert.Equal(heroes, setup.EffectiveHeroCount);
            Assert.Equal(twists, setup.EffectiveTwists);
        }

        [Fact]
        public void Organized_crime_wave_forces_maggia_goons()
        {
            var pool = CardPool.From([Set("core"), Set("dark-city")]);
            var setup = WithScheme(Rng(9), pool, 3, "dc:organized-crimewave");
            Assert.Contains(setup.Henchmen, h => h.Card.Id == "dc:maggia-goons" && h.IsRequired);
        }

        [Fact]
        public void Xcutioners_song_adds_an_extra_hero_and_no_bystanders()
        {
            var pool = CardPool.From([Set("core"), Set("dark-city")]);
            var setup = WithScheme(Rng(10), pool, 2, "dc:xcutioners-song");
            Assert.Equal(6, setup.EffectiveHeroCount);   // 5 base + 1
            Assert.Equal(0, setup.EffectiveBystanders);
        }

        [Theory]
        [InlineData("ff:cosmic-rays", 6)]
        [InlineData("ff:force-field", 7)]
        [InlineData("ff:melted-glaciers", 8)]
        public void Fantastic_four_scheme_twist_counts_are_exact(string schemeId, int twists)
        {
            var pool = CardPool.From([Set("core"), Set("fantastic-four")]);
            Assert.Equal(twists, WithScheme(Rng(3), pool, 3, schemeId).EffectiveTwists);
        }

        [Fact]
        public void Kree_skrull_war_forces_kree_starforce_and_the_core_skrulls()
        {
            var pool = CardPool.From([Set("core"), Set("guardians-of-the-galaxy")]);
            var setup = WithScheme(Rng(2), pool, 4, "gotg:kree-skrull-war");
            Assert.Contains(setup.VillainGroups, v => v.Card.Id == "gotg:kree-starforce" && v.IsRequired);
            Assert.Contains(setup.VillainGroups, v => v.Card.Id == "core:skrulls" && v.IsRequired);
        }

        [Fact]
        public void Forge_the_infinity_gauntlet_forces_infinity_gems()
        {
            var pool = CardPool.From([Set("core"), Set("guardians-of-the-galaxy")]);
            var setup = WithScheme(Rng(5), pool, 3, "gotg:infinity-gauntlet");
            Assert.Contains(setup.VillainGroups, v => v.Card.Id == "gotg:infinity-gems" && v.IsRequired);
        }

        [Theory]
        [InlineData(2, 7)]
        [InlineData(3, 8)]
        [InlineData(5, 10)]
        public void Unite_the_shards_twists_scale_with_players(int players, int twists)
        {
            var pool = CardPool.From([Set("core"), Set("guardians-of-the-galaxy")]);
            Assert.Equal(twists, WithScheme(Rng(6), pool, players, "gotg:unite-the-shards").EffectiveTwists);
        }

        [Fact]
        public void Splice_humans_forces_the_sinister_six_and_web_of_lies_uses_seven_twists()
        {
            var pool = CardPool.From([Set("core"), Set("paint-the-town-red")]);
            var splice = WithScheme(Rng(4), pool, 3, "ptr:splice-spider-dna");
            Assert.Contains(splice.VillainGroups, v => v.Card.Id == "ptr:sinister-six" && v.IsRequired);

            var web = WithScheme(Rng(4), pool, 3, "ptr:web-of-lies");
            Assert.Equal(7, web.EffectiveTwists);
        }

        [Fact]
        public void Villains_box_plays_standalone_and_never_mixes_other_sets_cards()
        {
            var pool = CardPool.From([Set("villains")]);
            for (var i = 0; i < 200; i++)
            {
                var setup = Rng(i).Generate((i % 5) + 1, pool);
                foreach (var card in setup.VillainGroups.Concat(setup.Henchmen).Concat(setup.Heroes)
                             .Append(setup.Mastermind).Append(setup.Scheme))
                    Assert.StartsWith("villains:", card.Card.Id);
            }
        }

        [Fact]
        public void Mass_produce_war_machine_forces_the_shield_assault_squad()
        {
            var pool = CardPool.From([Set("villains")]);
            var setup = WithScheme(Rng(7), pool, 4, "villains:mass-produce-war-machine");
            Assert.Contains(setup.Henchmen, h => h.Card.Id == "villains:shield-assault-squad" && h.IsRequired);
        }

        [Fact]
        public void Mark_of_khonshu_forces_its_guardians_and_the_fountain_scales_twists_solo()
        {
            var pool = CardPool.From([Set("secret-wars-vol-2")]);
            var setup = WithScheme(Rng(9), pool, 4, "sw2:mark-of-khonshu");
            Assert.Contains(setup.Henchmen, h => h.Card.Id == "sw2:khonshu-guardians" && h.IsRequired);
            Assert.Equal(10, setup.EffectiveTwists);

            // The Fountain of Eternal Life drops to 4 Twists solo, 8 otherwise.
            Assert.Equal(4, WithScheme(Rng(1), pool, 1, "sw2:fountain-eternal-life").EffectiveTwists);
            Assert.Equal(8, WithScheme(Rng(1), pool, 3, "sw2:fountain-eternal-life").EffectiveTwists);
        }

        [Fact]
        public void Epic_civil_war_scales_twists_with_players()
        {
            var pool = CardPool.From([Set("core"), Set("civil-war")]);
            Assert.Equal(9, WithScheme(Rng(3), pool, 3, "cw:epic-civil-war").EffectiveTwists);
            Assert.Equal(6, WithScheme(Rng(3), pool, 5, "cw:epic-civil-war").EffectiveTwists);
        }

        private static void AssertDistinct(IReadOnlyList<SetupSelection> items)
        {
            var ids = items.Select(i => i.Card.Id).ToList();
            Assert.Equal(ids.Count, ids.Distinct().Count());
        }
    }

    public class Reroll
    {
        [Fact]
        public void Mastermind_reroll_keeps_the_other_categories_stable()
        {
            var r = Rng(7);
            var pool = CorePool();
            var setup = r.Generate(4, pool);

            var scheme = setup.Scheme.Card.Id;
            var heroes = setup.Heroes.Select(h => h.Card.Id).OrderBy(x => x).ToList();

            var rerolled = r.Reroll(setup, CardCategory.Mastermind, pool);

            Assert.Equal(scheme, rerolled.Scheme.Card.Id);
            Assert.Equal(heroes, rerolled.Heroes.Select(h => h.Card.Id).OrderBy(x => x).ToList());
        }

        [Fact]
        public void Villain_reroll_rehonours_the_required_group()
        {
            var r = Rng(3);
            var pool = CorePool();
            var setup = r.Generate(3, pool);

            for (var i = 0; i < 100; i++)
            {
                setup = r.Reroll(setup, CardCategory.VillainGroup, pool);
                var mm = (Mastermind)setup.Mastermind.Card;
                var requiredIsVillain = pool.FindById(mm.AlwaysLeadsGroupId)?.Category == CardCategory.VillainGroup;
                if (requiredIsVillain)
                    Assert.Contains(setup.VillainGroups, v => v.Card.Id == mm.AlwaysLeadsGroupId && v.IsRequired);
            }
        }

        [Fact]
        public void Henchmen_reroll_rehonours_Dr_Dooms_Doombot_Legion()
        {
            // Force Dr. Doom (leads Doombot Legion, a Henchman group) then reroll henchmen repeatedly.
            var r = Rng(1);
            var pool = CorePool();
            var setup = r.Generate(2, pool);
            while (setup.Mastermind.Card.Id != "core:dr-doom")
                setup = r.Reroll(setup, CardCategory.Mastermind, pool);

            for (var i = 0; i < 100; i++)
            {
                setup = r.Reroll(setup, CardCategory.Henchmen, pool);
                Assert.Contains(setup.Henchmen, h => h.Card.Id == "core:doombot-legion" && h.IsRequired);
            }
        }
    }
}
