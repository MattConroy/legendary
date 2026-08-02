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

    /// <summary>Wounds available in the Wound stack.</summary>
    public required int Wounds { get; init; }
}

/// <summary>
/// Official per-player setup table for the Core Set.
/// (2–5 players are from the rulebook; 1-player uses the published solo variant.)
/// </summary>
public static class SetupTable
{
    public static readonly IReadOnlyDictionary<int, SetupRule> ByPlayers = new Dictionary<int, SetupRule>
    {
        [1] = new() { Players = 1, Heroes = 3, VillainGroups = 1, Henchmen = 1, MasterStrikes = 3, Bystanders = 1, Wounds = 12 },
        [2] = new() { Players = 2, Heroes = 5, VillainGroups = 2, Henchmen = 1, MasterStrikes = 5, Bystanders = 2, Wounds = 18 },
        [3] = new() { Players = 3, Heroes = 5, VillainGroups = 3, Henchmen = 1, MasterStrikes = 5, Bystanders = 8, Wounds = 27 },
        [4] = new() { Players = 4, Heroes = 5, VillainGroups = 3, Henchmen = 2, MasterStrikes = 5, Bystanders = 8, Wounds = 36 },
        [5] = new() { Players = 5, Heroes = 6, VillainGroups = 4, Henchmen = 2, MasterStrikes = 5, Bystanders = 12, Wounds = 45 },
    };

    public static SetupRule For(int players)
    {
        players = Math.Clamp(players, 1, 5);
        return ByPlayers[players];
    }
}
