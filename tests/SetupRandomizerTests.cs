using Legendary.Companion.Data;
using Legendary.Companion.Models;
using Legendary.Companion.Services;
using Xunit;

namespace Legendary.Companion.Tests;

public class SetupRandomizerTests
{
    // Deterministic RNG so assertions are stable.
    private static SetupRandomizer Rng(int seed = 12345) => new(new Random(seed));

    // Load the real content from the shipped JSON (copied next to the test binary).
    private static readonly IReadOnlyList<CardSet> Sets =
        SetJson.Deserialize(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "sets.json")));

    private static CardSet Set(string id) => Sets.First(s => s.Id == id);

    private static CardPool CorePool() => CardPool.From([Set("core")]);
    private static CardPool AllPool() => CardPool.From(Sets);

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

    [Fact]
    public void Disabling_a_set_removes_its_cards_from_the_pool()
    {
        var core = CorePool();
        var all = AllPool();
        Assert.True(all.Heroes.Count > core.Heroes.Count);
        // A Dark City card is present in the full pool but not in a Core-only pool.
        Assert.Contains(all.Heroes, h => h.Id == "dc:bishop");
        Assert.DoesNotContain(core.Heroes, h => h.Id == "dc:bishop");
    }

    [Fact]
    public void Every_mastermind_always_leads_group_resolves_within_the_full_pool()
    {
        var pool = AllPool();
        foreach (var set in Sets)
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
        var dc = Set("dark-city");
        Assert.Equal(5, dc.Masterminds.Count);
        Assert.Equal(8, dc.Schemes.Count);
        Assert.Equal(6, dc.VillainGroups.Count);
        Assert.Equal(2, dc.Henchmen.Count);
        Assert.Equal(17, dc.Heroes.Count);
    }

    [Fact]
    public void FantasticFour_has_the_expected_roster_and_always_leads()
    {
        var ff = Set("fantastic-four");
        Assert.Equal(2, ff.Masterminds.Count);
        Assert.Equal(4, ff.Schemes.Count);
        Assert.Equal(2, ff.VillainGroups.Count);
        Assert.Empty(ff.Henchmen);
        Assert.Equal(5, ff.Heroes.Count);

        Assert.Equal("ff:heralds-of-galactus", ff.Masterminds.Single(m => m.Name == "Galactus").AlwaysLeadsGroupId);
        Assert.Equal("ff:subterranea", ff.Masterminds.Single(m => m.Name == "Mole Man").AlwaysLeadsGroupId);
    }

    [Theory]
    [InlineData("ff:cosmic-rays", 6)]
    [InlineData("ff:force-field", 7)]
    [InlineData("ff:melted-glaciers", 8)]
    public void FantasticFour_scheme_twist_counts_are_exact(string schemeId, int twists)
    {
        var pool = CardPool.From([Set("core"), Set("fantastic-four")]);
        var setup = WithScheme(Rng(3), pool, 3, schemeId);
        Assert.Equal(twists, setup.EffectiveTwists);
    }

    [Fact]
    public void Guardians_has_the_expected_roster_and_always_leads()
    {
        var g = Set("guardians-of-the-galaxy");
        Assert.Equal(2, g.Masterminds.Count);
        Assert.Equal(4, g.Schemes.Count);
        Assert.Equal(2, g.VillainGroups.Count);
        Assert.Empty(g.Henchmen);
        Assert.Equal(5, g.Heroes.Count);

        Assert.Equal("gotg:kree-starforce", g.Masterminds.Single(m => m.Name.StartsWith("Supreme")).AlwaysLeadsGroupId);
        Assert.Equal("gotg:infinity-gems", g.Masterminds.Single(m => m.Name == "Thanos").AlwaysLeadsGroupId);
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
        var setup = WithScheme(Rng(6), pool, players, "gotg:unite-the-shards");
        Assert.Equal(twists, setup.EffectiveTwists);
    }

    [Fact]
    public void PaintTheTownRed_has_the_expected_roster_and_always_leads()
    {
        var p = Set("paint-the-town-red");
        Assert.Equal(2, p.Masterminds.Count);
        Assert.Equal(4, p.Schemes.Count);
        Assert.Equal(2, p.VillainGroups.Count);
        Assert.Empty(p.Henchmen);
        Assert.Equal(5, p.Heroes.Count);

        Assert.Equal("ptr:maximum-carnage", p.Masterminds.Single(m => m.Name == "Carnage").AlwaysLeadsGroupId);
        Assert.Equal("ptr:sinister-six", p.Masterminds.Single(m => m.Name == "Mysterio").AlwaysLeadsGroupId);
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
    public void Villains_has_the_expected_role_inverted_roster()
    {
        var v = Set("villains");
        Assert.True(v.Standalone);
        Assert.Equal(4, v.Masterminds.Count);   // Commanders
        Assert.Equal(8, v.Schemes.Count);       // Plots
        Assert.Equal(7, v.VillainGroups.Count); // Adversary groups
        Assert.Equal(4, v.Henchmen.Count);      // Backup adversaries
        Assert.Equal(15, v.Heroes.Count);       // Allies

        Assert.Equal("villains:avengers", v.Masterminds.Single(m => m.Name == "Nick Fury").AlwaysLeadsGroupId);
        // A Commander may lead a Backup-Adversary (Henchman) group, like Dr. Doom leads Doombot Legion in Core.
        Assert.Equal("villains:asgardian-warriors", v.Masterminds.Single(m => m.Name == "Odin").AlwaysLeadsGroupId);
    }

    [Fact]
    public void Villains_plays_standalone_and_never_mixes_other_sets_cards()
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

    private static void AssertDistinct(IReadOnlyList<SetupSelection> items)
    {
        var ids = items.Select(i => i.Card.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }
}
