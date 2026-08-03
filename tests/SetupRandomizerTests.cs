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
                // A null id is the "Any Villain Group" mechanic — no single group is forced.
                if (string.IsNullOrEmpty(mm.AlwaysLeadsGroupId)) continue;
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

    [Fact]
    public void SecretWars1_roster_and_always_leads()
    {
        var s = Set("secret-wars-vol-1");
        Assert.False(s.Standalone);
        Assert.Equal(4, s.Masterminds.Count);
        Assert.Equal(8, s.Schemes.Count);
        Assert.Equal(6, s.VillainGroups.Count);
        Assert.Equal(3, s.Henchmen.Count);
        Assert.Equal(14, s.Heroes.Count);
        Assert.Equal("sw1:the-deadlands", s.Masterminds.Single(m => m.Name == "Zombie Green Goblin").AlwaysLeadsGroupId);
    }

    [Fact]
    public void SecretWars2_roster_forces_and_backup_leading()
    {
        var s = Set("secret-wars-vol-2");
        Assert.Equal(4, s.Masterminds.Count);
        Assert.Equal(8, s.Schemes.Count);
        Assert.Equal(6, s.VillainGroups.Count);
        Assert.Equal(4, s.Henchmen.Count);
        Assert.Equal(16, s.Heroes.Count);

        // Spider-Queen leads a Backup-Adversary (Henchman) group, like Odin leads Asgardian Warriors.
        Assert.Equal("sw2:spider-infected", s.Masterminds.Single(m => m.Name == "Spider-Queen").AlwaysLeadsGroupId);

        var pool = CardPool.From([Set("secret-wars-vol-2")]);
        var setup = WithScheme(Rng(9), pool, 4, "sw2:mark-of-khonshu");
        Assert.Contains(setup.Henchmen, h => h.Card.Id == "sw2:khonshu-guardians" && h.IsRequired);
        Assert.Equal(10, setup.EffectiveTwists);

        // The Fountain of Eternal Life drops to 4 Twists solo, 8 otherwise.
        Assert.Equal(4, WithScheme(Rng(1), pool, 1, "sw2:fountain-eternal-life").EffectiveTwists);
        Assert.Equal(8, WithScheme(Rng(1), pool, 3, "sw2:fountain-eternal-life").EffectiveTwists);
    }

    [Fact]
    public void CivilWar_roster_and_player_scaled_twists()
    {
        var s = Set("civil-war");
        Assert.Equal(5, s.Masterminds.Count);
        Assert.Equal(8, s.Schemes.Count);
        Assert.Equal(7, s.VillainGroups.Count);
        Assert.Equal(2, s.Henchmen.Count);
        Assert.Equal(16, s.Heroes.Count);

        var pool = CardPool.From([Set("core"), Set("civil-war")]);
        Assert.Equal(9, WithScheme(Rng(3), pool, 3, "cw:epic-civil-war").EffectiveTwists);
        Assert.Equal(6, WithScheme(Rng(3), pool, 5, "cw:epic-civil-war").EffectiveTwists);
    }

    [Fact]
    public void CaptainAmerica75_and_Deadpool_rosters()
    {
        var cap = Set("captain-america-75");
        Assert.Equal(2, cap.Masterminds.Count);
        Assert.Equal(4, cap.Schemes.Count);
        Assert.Equal(2, cap.VillainGroups.Count);
        Assert.Equal(5, cap.Heroes.Count);

        var dp = Set("deadpool");
        Assert.Equal(2, dp.Masterminds.Count);
        Assert.Equal(4, dp.Schemes.Count);
        Assert.Equal(5, dp.Heroes.Count);
        Assert.All(dp.Heroes.Where(h => h.Name != "Bob, Agent of HYDRA"),
            h => Assert.Equal("Mercs for Money", ((Hero)h).Team));
    }

    [Fact]
    public void XMen_and_WorldWarHulk_big_box_rosters()
    {
        var xm = Set("x-men-box");
        Assert.Equal(6, xm.Masterminds.Count);   // base only, Epic variants excluded
        Assert.Equal(8, xm.Schemes.Count);
        Assert.Equal(15, xm.Heroes.Count);
        Assert.All(xm.Heroes, h => Assert.Equal("X-Men", ((Hero)h).Team));

        var wwh = Set("world-war-hulk");
        Assert.Equal(6, wwh.Masterminds.Count);   // Transformed flip-sides excluded
        Assert.Equal(15, wwh.Heroes.Count);
    }

    [Fact]
    public void Dimensions_is_a_mix_in_not_playable_alone()
    {
        var dim = Set("dimensions");
        Assert.Empty(dim.Schemes);
        Assert.Empty(dim.VillainGroups);
        // Its Mastermind leads a Backup-Adversary (Henchman) group.
        Assert.Equal("dim:spider-slayer", dim.Masterminds.Single().AlwaysLeadsGroupId);

        Assert.False(CardPool.From([dim]).IsPlayable);          // no schemes/villains on its own
        Assert.True(CardPool.From([Set("core"), dim]).IsPlayable); // folds into another set
    }

    [Fact]
    public void Every_hero_team_has_a_badge_mapping()
    {
        // Mirror of TeamIcon's slug map — every team a hero uses must resolve to a real badge,
        // otherwise it silently renders the Unaffiliated fallback.
        var known = new HashSet<string>
        {
            "Avengers", "Fantastic Four", "Guardians of the Galaxy", "X-Men", "X-Force",
            "Marvel Knights", "S.H.I.E.L.D.", "Spider-Friends", "Sinister Six", "Brotherhood",
            "Foes of Asgard", "Crime Syndicate", "Illuminati", "Cabal", "HYDRA", "New Warriors",
            "Mercs for Money", "Champions", "Warbound", "Venomverse", "Unaffiliated",
            "Heroes of Asgard", "Inhumans", "X-Factor", "Heroes of Wakanda",
            "Guardians of the Multiverse",
        };
        // A null team maps to the Unaffiliated badge, so only named teams need a mapping.
        var used = Sets.SelectMany(s => s.Heroes).Select(h => h.Team).OfType<string>().Distinct();
        foreach (var team in used)
            Assert.True(known.Contains(team), $"Hero team '{team}' has no badge mapping.");
    }

    [Fact]
    public void Full_catalog_reaches_the_marvel_studios_small_boxes()
    {
        // The catalogue runs release order from the Core Set through the late "small box" line.
        Assert.Equal(38, Sets.Count);
        foreach (var id in new[]
        {
            "revelations", "s-h-i-e-l-d", "heroes-of-asgard", "into-the-cosmos", "black-panther",
            "new-mutants", "realm-of-kings", "annihilation", "messiah-complex",
            "doctor-strange-and-the-shadows-of-nightmare", "black-widow",
            "marvel-studios-guardians-of-the-galaxy", "marvel-studios-what-if", "2099",
            "ant-man-and-the-wasp", "midnight-sons", "weapon-x",
        })
            Assert.Contains(Sets, s => s.Id == id);
    }

    [Fact]
    public void The_small_boxes_are_expansions_not_standalone()
    {
        // Per the wiki, the Marvel Studios boxes are Core Sets; every other small box is a mix-in.
        foreach (var id in new[]
        {
            "revelations", "s-h-i-e-l-d", "heroes-of-asgard", "into-the-cosmos", "black-panther",
            "new-mutants", "realm-of-kings", "annihilation", "messiah-complex",
            "doctor-strange-and-the-shadows-of-nightmare", "black-widow",
            "marvel-studios-guardians-of-the-galaxy", "2099",
            "ant-man-and-the-wasp", "midnight-sons", "weapon-x",
        })
            Assert.False(Set(id).Standalone, $"{id} should be a non-standalone expansion.");
    }

    [Fact]
    public void The_four_core_sets_are_standalone()
    {
        // Wiki "Core Sets": each plays on its own.
        foreach (var id in new[] { "core", "villains", "marvel-studios-phase-1", "marvel-studios-what-if" })
        {
            Assert.True(Set(id).Standalone, $"{id} should be a standalone Core Set.");
            Assert.True(CardPool.From([Set(id)]).IsPlayable, $"{id} should be playable alone.");
        }
    }

    [Fact]
    public void Transforming_schemes_are_a_single_pick_not_two()
    {
        // A double-sided ("Transforms") Scheme is one card, so only its front side is a choice.
        Assert.DoesNotContain(Sets.SelectMany(s => s.Schemes), sc => sc.Name.StartsWith('…') || sc.Name.StartsWith('”'));
        Assert.Equal(4, Set("revelations").Schemes.Count);          // Tsunami / No More Mutants / Open HYDRA / Korvac Revealed are reverse sides
        Assert.Equal(4, Set("messiah-complex").Schemes.Count);      // four Veiled schemes, their Unveiled reverses folded in
        Assert.Equal(4, Set("midnight-sons").Schemes.Count);        // Great Old One Chthon is the reverse of the Ritual Sacrifice scheme
        Assert.Contains(Set("messiah-complex").Schemes, sc => sc.Name == "Hack Cerebro Servers to Control the Mutant Messiah");
    }

    [Fact]
    public void WhatIf_roster_and_any_villain_group_mastermind()
    {
        var w = Set("marvel-studios-what-if");
        Assert.Equal(4, w.Masterminds.Count);
        Assert.Equal(4, w.Schemes.Count);
        Assert.Equal(5, w.VillainGroups.Count);
        Assert.Equal(3, w.Henchmen.Count);
        Assert.Equal(8, w.Heroes.Count);

        // Hank Pym leads "Any Villain Group" — no forced group.
        Assert.Null(w.Masterminds.Single(m => m.Name == "Hank Pym, Yellowjacket").AlwaysLeadsGroupId);
        // Two masterminds are led by Henchmen groups from this box.
        Assert.Equal("wif:ultron-sentries", w.Masterminds.Single(m => m.Name == "Ultron Infinity").AlwaysLeadsGroupId);
        Assert.Contains(w.Heroes, h => h.Name == "Captain Carter" && h.Team == "Guardians of the Multiverse");
    }

    [Fact]
    public void Transformed_masterminds_are_not_separate_picks()
    {
        // Yellowjacket, Ghost's intangible form and Kang's multiverse form are flip sides, not picks.
        var m = Set("ant-man-and-the-wasp").Masterminds;
        Assert.Equal(3, m.Count);
        Assert.DoesNotContain(m, x => x.Name is "Yellowjacket" or "Ghost, Intangible" or "Kang, Multiverse Conqueror");
    }

    private static void AssertDistinct(IReadOnlyList<SetupSelection> items)
    {
        var ids = items.Select(i => i.Card.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }
}
