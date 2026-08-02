using Legendary.Companion.Data;
using Legendary.Companion.Models;
using Legendary.Companion.Services;
using Xunit;

namespace Legendary.Companion.Tests;

public class SetupRandomizerTests
{
    // Deterministic RNG so assertions are stable.
    private static SetupRandomizer Rng(int seed = 12345) => new(new Random(seed));

    private static CardPool CorePool() => CardPool.From([CoreSet.Set]);
    private static CardPool AllPool() => CardPool.From(SetRegistry.AllSets);

    // Force a specific Scheme so a ruling can be asserted deterministically.
    private static GameSetup WithScheme(SetupRandomizer r, CardPool pool, int players, string schemeId)
    {
        var s = r.Generate(players, pool);
        for (var i = 0; i < 3000 && s.Scheme.Card.Id != schemeId; i++)
            s = r.Reroll(s, CardCategory.Scheme, pool);
        Assert.Equal(schemeId, s.Scheme.Card.Id);
        return s;
    }

    [Theory]
    [InlineData(1, 3, 1, 1, 3, 2)]
    [InlineData(2, 5, 2, 1, 5, 2)]
    [InlineData(3, 5, 3, 1, 5, 8)]
    [InlineData(4, 5, 3, 2, 5, 8)]
    [InlineData(5, 6, 4, 2, 5, 12)]
    public void Setup_table_matches_official_counts(int players, int heroes, int villains, int henchmen, int strikes, int bystanders)
    {
        var r = SetupTable.For(players);
        Assert.Equal(heroes, r.Heroes);
        Assert.Equal(villains, r.VillainGroups);
        Assert.Equal(henchmen, r.Henchmen);
        Assert.Equal(strikes, r.MasterStrikes);
        Assert.Equal(bystanders, r.Bystanders);
    }

    [Fact]
    public void Generate_effective_counts_match_selected_card_counts()
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

    [Theory]
    [InlineData("core:dark-dimension", 7)]     // Portals to the Dark Dimension
    [InlineData("core:killbots", 5)]           // Replace Earth's Leaders with Killbots
    [InlineData("core:legacy-virus", 8)]
    public void Scheme_twist_counts_are_exact(string schemeId, int twists)
    {
        var setup = WithScheme(Rng(1), CorePool(), 3, schemeId);
        Assert.Equal(twists, setup.EffectiveTwists);
    }

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
        var pool = CardPool.From([CoreSet.Set, DarkCity.Set]);
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
        var pool = CardPool.From([CoreSet.Set, DarkCity.Set]);
        var setup = WithScheme(Rng(9), pool, 3, "dc:organized-crimewave");
        Assert.Contains(setup.Henchmen, h => h.Card.Id == "dc:maggia-goons" && h.IsRequired);
    }

    [Fact]
    public void Xcutioners_song_adds_an_extra_hero_and_no_bystanders()
    {
        var pool = CardPool.From([CoreSet.Set, DarkCity.Set]);
        var setup = WithScheme(Rng(10), pool, 2, "dc:xcutioners-song");
        Assert.Equal(6, setup.EffectiveHeroCount);   // 5 base + 1
        Assert.Equal(0, setup.EffectiveBystanders);
    }

    [Fact]
    public void Generate_always_includes_masterminds_always_leads_group_marked_required()
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
    public void Generate_produces_distinct_cards_within_each_category()
    {
        var setup = Rng().Generate(5, AllPool());
        AssertDistinct(setup.VillainGroups);
        AssertDistinct(setup.Henchmen);
        AssertDistinct(setup.Heroes);
    }

    [Fact]
    public void Reroll_mastermind_keeps_other_categories_stable()
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
    public void Reroll_villains_rehonours_required_group()
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
            {
                Assert.Contains(setup.VillainGroups, v => v.Card.Id == mm.AlwaysLeadsGroupId && v.IsRequired);
            }
        }
    }

    [Fact]
    public void Reroll_henchmen_rehonours_doom_doombot_requirement()
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

    [Fact]
    public void Scheme_setup_deltas_change_the_effective_counts()
    {
        // Uses a synthetic set so the mechanism is tested without asserting any
        // real-game ruling (shipped Schemes currently declare no deltas).
        const string s = "t";
        var set = new CardSet
        {
            Id = s, Name = "Test",
            Masterminds = [new() { Id = "t:m", SetId = s, Name = "M", AlwaysLeadsGroupId = "t:v1" }],
            Schemes = [new() { Id = "t:s", SetId = s, Name = "S", Setup = new SchemeSetup { HenchmenDelta = 1, HeroDelta = 1 } }],
            VillainGroups = [new() { Id = "t:v1", SetId = s, Name = "V1" }, new() { Id = "t:v2", SetId = s, Name = "V2" }, new() { Id = "t:v3", SetId = s, Name = "V3" }],
            Henchmen = [new() { Id = "t:h1", SetId = s, Name = "H1" }, new() { Id = "t:h2", SetId = s, Name = "H2" }, new() { Id = "t:h3", SetId = s, Name = "H3" }],
            Heroes = Enumerable.Range(1, 8).Select(i => new Hero { Id = $"t:hero{i}", SetId = s, Name = $"Hero {i}" }).ToList(),
        };
        var pool = CardPool.From([set]);

        var setup = new SetupRandomizer(new Random(1)).Generate(2, pool); // 2p base: 5 heroes, 1 henchman
        Assert.Equal(6, setup.Heroes.Count);        // 5 + HeroDelta 1
        Assert.Equal(2, setup.Henchmen.Count);      // 1 + HenchmenDelta 1
        Assert.Equal(6, setup.EffectiveHeroCount);
        Assert.Equal(2, setup.EffectiveHenchmenCount);
    }

    [Fact]
    public void Disabling_a_set_removes_its_cards_from_the_pool()
    {
        var core = CorePool();
        var all = AllPool();
        Assert.True(all.Heroes.Count > core.Heroes.Count);
        Assert.DoesNotContain(core.Heroes, h => h.Id == "example:hero-a");
    }

    [Fact]
    public void Every_mastermind_always_leads_group_resolves_within_the_full_pool()
    {
        var pool = AllPool();
        foreach (var set in SetRegistry.AllSets)
        {
            foreach (var mm in set.Masterminds)
            {
                Assert.False(string.IsNullOrEmpty(mm.AlwaysLeadsGroupId),
                    $"{mm.Name} has no AlwaysLeadsGroupId");
                var group = pool.FindById(mm.AlwaysLeadsGroupId);
                Assert.True(group is VillainGroup or Henchmen,
                    $"{mm.Name} always-leads id '{mm.AlwaysLeadsGroupId}' does not resolve to a group");
            }
        }
    }

    [Fact]
    public void DarkCity_has_the_expected_roster_sizes()
    {
        var dc = SetRegistry.FindById("dark-city")!;
        Assert.Equal(5, dc.Masterminds.Count);
        Assert.Equal(8, dc.Schemes.Count);
        Assert.Equal(6, dc.VillainGroups.Count);
        Assert.Equal(2, dc.Henchmen.Count);
        Assert.Equal(17, dc.Heroes.Count);
    }

    private static void AssertDistinct(IReadOnlyList<SetupSelection> items)
    {
        var ids = items.Select(i => i.Card.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }
}
