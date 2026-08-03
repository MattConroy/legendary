using Legendary.Companion.Models;

namespace Legendary.Companion.Services;

/// <summary>
/// Generates and rerolls Legendary game setups following the official setup rules.
/// It is deliberately set-agnostic: it only reads the aggregated <see cref="CardPool"/>
/// and each Scheme's declared <see cref="SchemeSetup"/>, so enabling or disabling an
/// expansion (or adding a new Scheme rule) changes data, never this code.
/// </summary>
public sealed class SetupRandomizer
{
    private readonly Random _rng;

    public SetupRandomizer(Random? rng = null) => _rng = rng ?? Random.Shared;

    /// <summary>Generate a complete, fresh setup for the given player count.</summary>
    public GameSetup Generate(int players, CardPool pool)
    {
        var rule = SetupTable.For(players);
        var mastermind = PickOne(pool.Masterminds);
        var scheme = PickOne(pool.Schemes);

        var counts = EffectiveCounts(rule, scheme, players, mastermind, pool);

        var villains = DrawGroups(pool.VillainGroups, counts.Villains, RequiredOf<VillainGroup>(mastermind, scheme, pool));
        var henchmen = DrawGroups(pool.Henchmen, counts.Henchmen, RequiredOf<Henchmen>(mastermind, scheme, pool));
        var heroes = Draw(pool.Heroes, counts.Heroes, new HashSet<string>());

        return Build(players, rule, mastermind, scheme, villains, henchmen, heroes, pool);
    }

    /// <summary>
    /// Reroll a single category, keeping every other category stable, then
    /// re-honour every required group (Mastermind's "Always Leads" and any group
    /// a Scheme forces) and reconcile counts the new Mastermind/Scheme may imply.
    /// </summary>
    public GameSetup Reroll(GameSetup current, CardCategory category, CardPool pool)
    {
        var rule = current.Rule;
        var players = current.Players;

        var mastermind = current.Mastermind.Card as Mastermind ?? PickOne(pool.Masterminds);
        var scheme = current.Scheme.Card as Scheme ?? PickOne(pool.Schemes);
        var villains = Cards<VillainGroup>(current.VillainGroups);
        var henchmen = Cards<Henchmen>(current.Henchmen);
        var heroes = Cards<Hero>(current.Heroes);

        switch (category)
        {
            case CardCategory.Mastermind:
                mastermind = PickOneDifferent(pool.Masterminds, mastermind);
                break;
            case CardCategory.Scheme:
                scheme = PickOneDifferent(pool.Schemes, scheme);
                break;
            case CardCategory.VillainGroup:
                villains = DrawGroups(pool.VillainGroups,
                    EffectiveCounts(rule, scheme, players, mastermind, pool).Villains,
                    RequiredOf<VillainGroup>(mastermind, scheme, pool));
                break;
            case CardCategory.Henchmen:
                henchmen = DrawGroups(pool.Henchmen,
                    EffectiveCounts(rule, scheme, players, mastermind, pool).Henchmen,
                    RequiredOf<Henchmen>(mastermind, scheme, pool));
                break;
            case CardCategory.Hero:
                heroes = Draw(pool.Heroes,
                    EffectiveCounts(rule, scheme, players, mastermind, pool).Heroes,
                    new HashSet<string>());
                break;
        }

        // A Mastermind or Scheme reroll can change the target counts and/or which
        // groups are required, so reconcile the group selections before rebuilding.
        var counts = EffectiveCounts(rule, scheme, players, mastermind, pool);
        villains = Reconcile(villains, pool.VillainGroups, counts.Villains, RequiredOf<VillainGroup>(mastermind, scheme, pool));
        henchmen = Reconcile(henchmen, pool.Henchmen, counts.Henchmen, RequiredOf<Henchmen>(mastermind, scheme, pool));
        heroes = Reconcile(heroes, pool.Heroes, counts.Heroes, []);

        return Build(players, rule, mastermind, scheme, villains, henchmen, heroes, pool);
    }

    // ----- effective-count computation (base table + scheme rules) -----

    private readonly record struct Counts(int Heroes, int Villains, int Henchmen);

    private static Counts EffectiveCounts(SetupRule rule, Scheme scheme, int players, Mastermind mastermind, CardPool pool)
    {
        var s = scheme.Setup;

        var heroes = Clamp(ResolveHeroes(rule, s, players), 1, pool.Heroes.Count);

        // A category must have room for every group forced into it.
        var reqVillains = RequiredOf<VillainGroup>(mastermind, scheme, pool).Count;
        var reqHenchmen = RequiredOf<Henchmen>(mastermind, scheme, pool).Count;

        var villains = Clamp(Math.Max(rule.VillainGroups + s.VillainGroupDelta, reqVillains), 1, pool.VillainGroups.Count);
        var henchmen = Clamp(Math.Max(rule.Henchmen + s.HenchmenDelta, reqHenchmen), 1, pool.Henchmen.Count);
        return new Counts(heroes, villains, henchmen);
    }

    private static int ResolveHeroes(SetupRule rule, SchemeSetup s, int players)
    {
        if (s.HeroesByPlayers is { } byPlayers && byPlayers.TryGetValue(players, out var v)) return v;
        if (s.Heroes is { } absolute) return absolute;
        return rule.Heroes + s.HeroDelta;
    }

    private static int ResolveTwists(SchemeSetup s, int players) =>
        s.TwistsByPlayers is { } byPlayers && byPlayers.TryGetValue(players, out var v) ? v : s.Twists;

    // ----- required-group resolution (Mastermind "Always Leads" + Scheme-forced) -----

    private static List<T> RequiredOf<T>(Mastermind mastermind, Scheme scheme, CardPool pool) where T : GameCard
    {
        var category = CategoryOf<T>();
        var ids = new List<string?> { mastermind.AlwaysLeadsGroupId };
        if (category == CardCategory.VillainGroup) ids.AddRange(scheme.Setup.RequiredVillainGroupIds);
        if (category == CardCategory.Henchmen) ids.AddRange(scheme.Setup.RequiredHenchmenGroupIds);

        var result = new List<T>();
        foreach (var id in ids)
        {
            if (pool.FindById(id) is T card && result.All(c => c.Id != card.Id))
                result.Add(card);
        }
        return result;
    }

    // ----- drawing helpers -----

    private List<T> DrawGroups<T>(IReadOnlyList<T> source, int count, IReadOnlyCollection<T> required) where T : GameCard
    {
        var seed = required.Where(r => source.Any(c => c.Id == r.Id)).ToList();
        var remaining = count - seed.Count;
        if (remaining > 0)
            seed.AddRange(Draw(source, remaining, seed.Select(s => s.Id).ToHashSet()));
        return seed.Take(Math.Max(count, seed.Count)).ToList();
    }

    private List<T> Draw<T>(IReadOnlyList<T> source, int count, ISet<string> exclude) where T : GameCard
    {
        var candidates = source.Where(c => !exclude.Contains(c.Id)).ToList();
        Shuffle(candidates);
        return candidates.Take(Math.Max(0, count)).ToList();
    }

    /// <summary>
    /// Grow/shrink a selection to the target count and guarantee every required
    /// group is present, disturbing as few existing picks as possible.
    /// </summary>
    private List<T> Reconcile<T>(List<T> current, IReadOnlyList<T> source, int target, IReadOnlyCollection<T> required) where T : GameCard
    {
        // Drop picks no longer in the pool (e.g. a set was disabled).
        var valid = current.Where(c => source.Any(s => s.Id == c.Id)).ToList();

        // Ensure every required group is present.
        foreach (var req in required.Where(r => source.Any(s => s.Id == r.Id)))
        {
            if (valid.Any(c => c.Id == req.Id)) continue;
            var replaceable = valid.FindLastIndex(c => required.All(r => r.Id != c.Id));
            if (valid.Count >= target && replaceable >= 0)
                valid[replaceable] = req;
            else
                valid.Add(req);
        }

        // Grow to target.
        if (valid.Count < target)
            valid.AddRange(Draw(source, target - valid.Count, valid.Select(c => c.Id).ToHashSet()));

        // Shrink to target, never dropping a required group.
        while (valid.Count > target)
        {
            var idx = valid.FindLastIndex(c => required.All(r => r.Id != c.Id));
            if (idx < 0) break;
            valid.RemoveAt(idx);
        }

        return valid;
    }

    private static CardCategory CategoryOf<T>() where T : GameCard => typeof(T) switch
    {
        var t when t == typeof(VillainGroup) => CardCategory.VillainGroup,
        var t when t == typeof(Henchmen) => CardCategory.Henchmen,
        var t when t == typeof(Hero) => CardCategory.Hero,
        _ => CardCategory.Mastermind,
    };

    // ----- assembly -----

    private static GameSetup Build(
        int players, SetupRule rule, Mastermind mastermind, Scheme scheme,
        List<VillainGroup> villains, List<Henchmen> henchmen, List<Hero> heroes, CardPool pool)
    {
        var counts = EffectiveCounts(rule, scheme, players, mastermind, pool);

        var requiredIds = RequiredOf<VillainGroup>(mastermind, scheme, pool).Select(c => c.Id)
            .Concat(RequiredOf<Henchmen>(mastermind, scheme, pool).Select(c => c.Id))
            .ToHashSet();

        return new GameSetup
        {
            Players = players,
            Rule = rule,
            Mastermind = new SetupSelection(mastermind),
            Scheme = new SetupSelection(scheme),
            VillainGroups = villains.Select(v => new SetupSelection(v, requiredIds.Contains(v.Id))).ToList(),
            Henchmen = henchmen.Select(h => new SetupSelection(h, requiredIds.Contains(h.Id))).ToList(),
            Heroes = heroes.Select(h => new SetupSelection(h)).ToList(),
            EffectiveHeroCount = counts.Heroes,
            EffectiveVillainGroupCount = counts.Villains,
            EffectiveHenchmenCount = counts.Henchmen,
            EffectiveTwists = ResolveTwists(scheme.Setup, players),
            EffectiveBystanders = scheme.Setup.Bystanders ?? rule.Bystanders,
        };
    }

    // ----- primitives -----

    private static List<T> Cards<T>(IReadOnlyList<SetupSelection> selections) where T : GameCard =>
        selections.Select(s => s.Card).OfType<T>().ToList();

    private T PickOne<T>(IReadOnlyList<T> source) => source[_rng.Next(source.Count)];

    private T PickOneDifferent<T>(IReadOnlyList<T> source, T current) where T : GameCard
    {
        if (source.Count <= 1) return source[0];
        T pick;
        do { pick = source[_rng.Next(source.Count)]; } while (pick.Id == current.Id);
        return pick;
    }

    private void Shuffle<T>(IList<T> list)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = _rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static int Clamp(int value, int min, int max) =>
        max < min ? min : Math.Clamp(value, min, max);
}
