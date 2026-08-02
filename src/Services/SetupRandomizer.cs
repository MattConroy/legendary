using Legendary.Companion.Models;

namespace Legendary.Companion.Services;

/// <summary>
/// Generates and rerolls Legendary game setups following the official setup rules.
/// It is deliberately set-agnostic: it only reads the aggregated <see cref="CardPool"/>,
/// so enabling or disabling an expansion changes the pool, never this code.
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

        var counts = EffectiveCounts(rule, scheme, pool);

        var villains = DrawGroups(pool.VillainGroups, counts.Villains, mastermind, pool, required: []);
        var henchmen = DrawGroups(pool.Henchmen, counts.Henchmen, mastermind, pool, required: []);
        var heroes = Draw(pool.Heroes, counts.Heroes, exclude: new HashSet<string>());

        return Build(players, rule, mastermind, scheme, villains, henchmen, heroes, pool);
    }

    /// <summary>
    /// Reroll a single category, keeping every other category stable, then
    /// re-honour the Mastermind's "Always Leads" required group.
    /// </summary>
    public GameSetup Reroll(GameSetup current, CardCategory category, CardPool pool)
    {
        var rule = current.Rule;

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
            {
                var counts = EffectiveCounts(rule, scheme, pool);
                villains = DrawGroups(pool.VillainGroups, counts.Villains, mastermind, pool, required: []);
                break;
            }

            case CardCategory.Henchmen:
            {
                var counts = EffectiveCounts(rule, scheme, pool);
                henchmen = DrawGroups(pool.Henchmen, counts.Henchmen, mastermind, pool, required: []);
                break;
            }

            case CardCategory.Hero:
            {
                var counts = EffectiveCounts(rule, scheme, pool);
                heroes = Draw(pool.Heroes, counts.Heroes, exclude: new HashSet<string>());
                break;
            }
        }

        // A Mastermind or Scheme reroll can change the target counts and/or the
        // required group, so reconcile the group selections before rebuilding.
        var effective = EffectiveCounts(rule, scheme, pool);
        villains = Reconcile(villains, pool.VillainGroups, effective.Villains, RequiredGroupIn(mastermind, CardCategory.VillainGroup, pool) as VillainGroup);
        henchmen = Reconcile(henchmen, pool.Henchmen, effective.Henchmen, RequiredGroupIn(mastermind, CardCategory.Henchmen, pool) as Henchmen);
        heroes = ReconcileSimple(heroes, pool.Heroes, effective.Heroes);

        return Build(current.Players, rule, mastermind, scheme, villains, henchmen, heroes, pool);
    }

    // ----- effective-count computation (base table + scheme modifiers) -----

    private readonly record struct Counts(int Heroes, int Villains, int Henchmen);

    private static Counts EffectiveCounts(SetupRule rule, Scheme scheme, CardPool pool)
    {
        var setup = scheme.Setup;
        var heroes = Clamp(rule.Heroes + setup.HeroDelta, 1, pool.Heroes.Count);
        var villains = Clamp(rule.VillainGroups + setup.VillainGroupDelta, 1, pool.VillainGroups.Count);
        var henchmen = Clamp(rule.Henchmen + setup.HenchmenDelta, 1, pool.Henchmen.Count);
        return new Counts(heroes, villains, henchmen);
    }

    // ----- drawing helpers -----

    private List<T> DrawGroups<T>(IReadOnlyList<T> source, int count, Mastermind mastermind, CardPool pool, IReadOnlyCollection<T> required)
        where T : GameCard
    {
        var forced = RequiredGroupIn(mastermind, CategoryOf<T>(), pool) as T;
        var seed = new List<T>();
        if (forced is not null && source.Any(c => c.Id == forced.Id))
            seed.Add(forced);
        seed.AddRange(required.Where(r => seed.All(s => s.Id != r.Id)));

        var remaining = count - seed.Count;
        if (remaining > 0)
            seed.AddRange(Draw(source, remaining, exclude: seed.Select(s => s.Id).ToHashSet()));

        return seed.Take(count).ToList();
    }

    private List<T> Draw<T>(IReadOnlyList<T> source, int count, ISet<string> exclude) where T : GameCard
    {
        var candidates = source.Where(c => !exclude.Contains(c.Id)).ToList();
        Shuffle(candidates);
        return candidates.Take(Math.Max(0, count)).ToList();
    }

    /// <summary>
    /// Grow/shrink a group selection to the target count and guarantee the
    /// required (Always-Leads) group is present, without disturbing more of the
    /// existing picks than necessary.
    /// </summary>
    private List<T> Reconcile<T>(List<T> current, IReadOnlyList<T> source, int target, T? requiredGroup) where T : GameCard
    {
        // Drop picks no longer in the pool (e.g. a set was disabled).
        var valid = current.Where(c => source.Any(s => s.Id == c.Id)).ToList();

        // Ensure required group present.
        if (requiredGroup is not null && valid.All(c => c.Id != requiredGroup.Id))
        {
            if (valid.Count >= target && valid.Count > 0)
            {
                // Replace a non-required existing pick to make room.
                var removable = valid.FindLastIndex(_ => true);
                valid[removable] = requiredGroup;
            }
            else
            {
                valid.Add(requiredGroup);
            }
        }

        // Grow to target.
        if (valid.Count < target)
        {
            var exclude = valid.Select(c => c.Id).ToHashSet();
            valid.AddRange(Draw(source, target - valid.Count, exclude));
        }

        // Shrink to target, but never drop the required group.
        while (valid.Count > target)
        {
            var idx = valid.FindLastIndex(c => requiredGroup is null || c.Id != requiredGroup.Id);
            if (idx < 0) break;
            valid.RemoveAt(idx);
        }

        return valid;
    }

    private List<T> ReconcileSimple<T>(List<T> current, IReadOnlyList<T> source, int target) where T : GameCard
    {
        var valid = current.Where(c => source.Any(s => s.Id == c.Id)).ToList();
        if (valid.Count < target)
        {
            var exclude = valid.Select(c => c.Id).ToHashSet();
            valid.AddRange(Draw(source, target - valid.Count, exclude));
        }
        else if (valid.Count > target)
        {
            valid = valid.Take(target).ToList();
        }
        return valid;
    }

    // ----- required-group resolution -----

    private static GameCard? RequiredGroupIn(Mastermind mastermind, CardCategory category, CardPool pool)
    {
        var group = pool.FindById(mastermind.AlwaysLeadsGroupId);
        return group is not null && group.Category == category ? group : null;
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
        var requiredId = mastermind.AlwaysLeadsGroupId;
        var counts = EffectiveCounts(rule, scheme, pool);

        // The "Always Leads" group is already conveyed by the Required badge, so it
        // is not repeated in the notes or on the Mastermind card.
        var notes = new List<string>(scheme.Setup.Notes);

        return new GameSetup
        {
            Players = players,
            Rule = rule,
            Mastermind = new SetupSelection(mastermind),
            Scheme = new SetupSelection(scheme),
            VillainGroups = villains.Select(v => new SetupSelection(v, v.Id == requiredId)).ToList(),
            Henchmen = henchmen.Select(h => new SetupSelection(h, h.Id == requiredId)).ToList(),
            Heroes = heroes.Select(h => new SetupSelection(h)).ToList(),
            EffectiveHeroCount = counts.Heroes,
            EffectiveVillainGroupCount = counts.Villains,
            EffectiveHenchmenCount = counts.Henchmen,
            Notes = notes,
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
