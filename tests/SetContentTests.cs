using Legendary.Companion.Models;
using Legendary.Companion.Services;
using Xunit;
using static Legendary.Companion.Tests.Content;

namespace Legendary.Companion.Tests;

/// <summary>
/// Validates the shipped <c>sets.json</c> content — roster shapes, standalone
/// flags, "Always Leads" references, flip-side handling, difficulty ratings and
/// team badges. These assert the data contract, not a single class's behaviour.
/// </summary>
public class SetContentTests
{
    public class Rosters
    {
        [Fact]
        public void Dark_city_has_the_expected_sizes()
        {
            var dc = Set("dark-city");
            Assert.Equal(5, dc.Masterminds.Count);
            Assert.Equal(8, dc.Schemes.Count);
            Assert.Equal(6, dc.VillainGroups.Count);
            Assert.Equal(2, dc.Henchmen.Count);
            Assert.Equal(17, dc.Heroes.Count);
        }

        [Fact]
        public void Fantastic_four_has_the_expected_roster_and_always_leads()
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
        public void Paint_the_town_red_has_the_expected_roster_and_always_leads()
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
        public void Villains_box_has_the_expected_role_inverted_roster()
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
        public void Secret_wars_vol_1_roster_and_always_leads()
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
        public void Secret_wars_vol_2_roster_and_backup_leading()
        {
            var s = Set("secret-wars-vol-2");
            Assert.Equal(4, s.Masterminds.Count);
            Assert.Equal(8, s.Schemes.Count);
            Assert.Equal(6, s.VillainGroups.Count);
            Assert.Equal(4, s.Henchmen.Count);
            Assert.Equal(16, s.Heroes.Count);

            // Spider-Queen leads a Backup-Adversary (Henchman) group, like Odin leads Asgardian Warriors.
            Assert.Equal("sw2:spider-infected", s.Masterminds.Single(m => m.Name == "Spider-Queen").AlwaysLeadsGroupId);
        }

        [Fact]
        public void Civil_war_roster()
        {
            var s = Set("civil-war");
            Assert.Equal(5, s.Masterminds.Count);
            Assert.Equal(8, s.Schemes.Count);
            Assert.Equal(7, s.VillainGroups.Count);
            Assert.Equal(2, s.Henchmen.Count);
            Assert.Equal(16, s.Heroes.Count);
        }

        [Fact]
        public void Captain_america_75_and_deadpool_rosters()
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
        public void X_men_and_world_war_hulk_big_box_rosters()
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
        public void What_if_roster_and_any_villain_group_mastermind()
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
        public void Dimensions_is_a_mix_in_with_a_backup_leading_mastermind()
        {
            var dim = Set("dimensions");
            Assert.Empty(dim.Schemes);
            Assert.Empty(dim.VillainGroups);
            // Its Mastermind leads a Backup-Adversary (Henchman) group.
            Assert.Equal("dim:spider-slayer", dim.Masterminds.Single().AlwaysLeadsGroupId);
        }
    }

    public class AlwaysLeadsReferences
    {
        [Fact]
        public void Every_mastermind_always_leads_group_resolves_within_the_full_pool()
        {
            var pool = AllPool();
            foreach (var mm in Sets.SelectMany(s => s.Masterminds))
            {
                // A null id is the "Any Villain Group" mechanic — no single group is forced.
                if (string.IsNullOrEmpty(mm.AlwaysLeadsGroupId)) continue;
                var group = pool.FindById(mm.AlwaysLeadsGroupId);
                Assert.True(group is VillainGroup or Henchmen,
                    $"{mm.Name} always-leads id '{mm.AlwaysLeadsGroupId}' does not resolve to a group");
            }
        }
    }

    public class StandaloneFlags
    {
        [Fact]
        public void The_marvel_studios_and_villains_boxes_are_standalone_core_sets()
        {
            // Wiki "Core Sets": each plays on its own.
            foreach (var id in new[] { "core", "villains", "marvel-studios-phase-1", "marvel-studios-what-if" })
            {
                Assert.True(Set(id).Standalone, $"{id} should be a standalone Core Set.");
                Assert.True(CardPool.From([Set(id)]).IsPlayable, $"{id} should be playable alone.");
            }
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
    }

    public class Catalogue
    {
        [Fact]
        public void Runs_release_order_through_the_marvel_studios_small_boxes()
        {
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
    }

    public class FlipSides
    {
        [Fact]
        public void Transforming_schemes_are_a_single_pick_not_two()
        {
            // A double-sided ("Transforms") Scheme is one card, so only its front side is a choice.
            Assert.DoesNotContain(Sets.SelectMany(s => s.Schemes), sc => sc.Name.StartsWith('…') || sc.Name.StartsWith('”'));
            Assert.Equal(4, Set("revelations").Schemes.Count);          // reverse sides folded in
            Assert.Equal(4, Set("messiah-complex").Schemes.Count);      // four Veiled schemes, Unveiled reverses folded in
            Assert.Equal(4, Set("midnight-sons").Schemes.Count);        // Great Old One Chthon is the reverse of Ritual Sacrifice
            Assert.Contains(Set("messiah-complex").Schemes, sc => sc.Name == "Hack Cerebro Servers to Control the Mutant Messiah");
        }

        [Fact]
        public void Transformed_masterminds_are_not_separate_picks()
        {
            // Yellowjacket, Ghost's intangible form and Kang's multiverse form are flip sides, not picks.
            var m = Set("ant-man-and-the-wasp").Masterminds;
            Assert.Equal(3, m.Count);
            Assert.DoesNotContain(m, x => x.Name is "Yellowjacket" or "Ghost, Intangible" or "Kang, Multiverse Conqueror");
        }
    }

    public class Difficulty
    {
        [Fact]
        public void Every_mastermind_and_scheme_is_rated_1_to_5()
        {
            foreach (var set in Sets)
            {
                foreach (var m in set.Masterminds)
                    Assert.InRange(m.Difficulty ?? -1, 1, 5);
                foreach (var sc in set.Schemes)
                    Assert.InRange(sc.Difficulty ?? -1, 1, 5);
            }
        }
    }

    public class Teams
    {
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
    }
}
