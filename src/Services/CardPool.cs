using Legendary.Companion.Models;

namespace Legendary.Companion.Services;

/// <summary>
/// The aggregate of all cards drawn from the currently-enabled sets. This is the
/// only view of content the randomiser has — it is completely unaware of which
/// sets exist or which are toggled on.
/// </summary>
public sealed class CardPool
{
    public required IReadOnlyList<Mastermind> Masterminds { get; init; }
    public required IReadOnlyList<Scheme> Schemes { get; init; }
    public required IReadOnlyList<VillainGroup> VillainGroups { get; init; }
    public required IReadOnlyList<Henchmen> Henchmen { get; init; }
    public required IReadOnlyList<Hero> Heroes { get; init; }

    private readonly Dictionary<string, GameCard> _byId = [];

    public static CardPool From(IEnumerable<CardSet> enabledSets)
    {
        var sets = enabledSets.ToList();
        var pool = new CardPool
        {
            Masterminds = sets.SelectMany(s => s.Masterminds).ToList(),
            Schemes = sets.SelectMany(s => s.Schemes).ToList(),
            VillainGroups = sets.SelectMany(s => s.VillainGroups).ToList(),
            Henchmen = sets.SelectMany(s => s.Henchmen).ToList(),
            Heroes = sets.SelectMany(s => s.Heroes).ToList(),
        };

        foreach (var card in sets.SelectMany(s => s.AllCards))
            pool._byId[card.Id] = card;

        return pool;
    }

    public GameCard? FindById(string? id) =>
        id is not null && _byId.TryGetValue(id, out var c) ? c : null;

    /// <summary>True when every category has at least one card to draw from.</summary>
    public bool IsPlayable =>
        Masterminds.Count > 0 && Schemes.Count > 0 &&
        VillainGroups.Count > 0 && Henchmen.Count > 0 && Heroes.Count > 0;
}
