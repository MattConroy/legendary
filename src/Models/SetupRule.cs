namespace Legendary.Companion.Models;

/// <summary>
/// The base counts of each randomised category for a given player count,
/// per the official Legendary setup table. Scheme setup modifiers are layered
/// on top of these at randomise time.
/// </summary>
public sealed record SetupRule
{
    public required int Players { get; init; }
    public required int Heroes { get; init; }
    public required int VillainGroups { get; init; }
    public required int Henchmen { get; init; }

    /// <summary>Number of Master Strike cards shuffled into the Villain Deck.</summary>
    public required int MasterStrikes { get; init; }

    /// <summary>Bystanders shuffled into the Villain Deck.</summary>
    public required int Bystanders { get; init; }

    /// <summary>
    /// Number of Scheme Twists shuffled into the Villain Deck in a standard game.
    /// Base and Dark City Schemes run an 8-Twist track (Twist 1 … 8 → "Evil Wins").
    /// Solo play and a few Schemes vary this, so the Scheme card is authoritative.
    /// </summary>
    public required int SchemeTwists { get; init; }
}

/// <summary>
/// Official per-player setup table for the Core Set.
/// (2–5 players are from the rulebook; 1-player uses the published solo variant.)
/// Master Strikes are a fixed 5 in a multiplayer game; Bystanders jump sharply at
/// 3 players; Scheme Twists are ultimately dictated by the Scheme card.
/// </summary>
public static class SetupTable
{
    public static readonly IReadOnlyDictionary<int, SetupRule> ByPlayers = new Dictionary<int, SetupRule>
    {
        [1] = new() { Players = 1, Heroes = 3, VillainGroups = 1, Henchmen = 1, MasterStrikes = 3, Bystanders = 2, SchemeTwists = 8 },
        [2] = new() { Players = 2, Heroes = 5, VillainGroups = 2, Henchmen = 1, MasterStrikes = 5, Bystanders = 2, SchemeTwists = 8 },
        [3] = new() { Players = 3, Heroes = 5, VillainGroups = 3, Henchmen = 1, MasterStrikes = 5, Bystanders = 8, SchemeTwists = 8 },
        [4] = new() { Players = 4, Heroes = 5, VillainGroups = 3, Henchmen = 2, MasterStrikes = 5, Bystanders = 8, SchemeTwists = 8 },
        [5] = new() { Players = 5, Heroes = 6, VillainGroups = 4, Henchmen = 2, MasterStrikes = 5, Bystanders = 12, SchemeTwists = 8 },
    };

    public static SetupRule For(int players)
    {
        players = Math.Clamp(players, 1, 5);
        return ByPlayers[players];
    }
}
