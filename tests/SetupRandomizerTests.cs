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

    [Theory]
    [InlineData(1, 3, 1, 1)]
    [InlineData(2, 5, 2, 1)]
    [InlineData(3, 5, 3, 1)]
    [InlineData(4, 5, 3, 2)]
    [InlineData(5, 6, 4, 2)]
    public void Generate_uses_official_counts_for_each_player_count(int players, int heroes, int villains, int henchmen)
    {
        var pool = CorePool();
        // Run many times to be robust against scheme modifiers landing.
        for (var i = 0; i < 200; i++)
        {
            var setup = Rng(i).Generate(players, pool);

            var schemeSetup = ((Scheme)setup.Scheme.Card).Setup;
            Assert.Equal(heroes + schemeSetup.HeroDelta, setup.Heroes.Count);
            Assert.Equal(villains + schemeSetup.VillainGroupDelta, setup.VillainGroups.Count);
            Assert.Equal(henchmen + schemeSetup.HenchmenDelta, setup.Henchmen.Count);
        }
    }

    [Fact]
    public void Generate_always_includes_masterminds_always_leads_group_marked_required()
    {
        var pool = CorePool();
        for (var i = 0; i < 500; i++)
        {
            var setup = Rng(i).Generate(3, pool);
            var mm = (Mastermind)setup.Mastermind.Card;
            var required = setup.VillainGroups.Concat(setup.Henchmen)
                .Where(s => s.IsRequired)
                .ToList();

            Assert.Single(required);
            Assert.Equal(mm.AlwaysLeadsGroupId, required[0].Card.Id);
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
