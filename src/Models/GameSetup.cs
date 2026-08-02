namespace Legendary.Companion.Models;

/// <summary>A single selected card in a generated setup, with its "required" state.</summary>
public sealed record SetupSelection(GameCard Card, bool IsRequired = false);

/// <summary>
/// A fully generated game setup. Immutable per-generation; rerolls produce a new
/// instance via the randomiser so Blazor re-renders cleanly.
/// </summary>
public sealed record GameSetup
{
    public required int Players { get; init; }
    public required SetupRule Rule { get; init; }

    public required SetupSelection Mastermind { get; init; }
    public required SetupSelection Scheme { get; init; }
    public required IReadOnlyList<SetupSelection> VillainGroups { get; init; }
    public required IReadOnlyList<SetupSelection> Henchmen { get; init; }
    public required IReadOnlyList<SetupSelection> Heroes { get; init; }

    /// <summary>
    /// Effective counts after applying the current Scheme's setup modifiers,
    /// so the UI can show the real targets.
    /// </summary>
    public required int EffectiveHeroCount { get; init; }
    public required int EffectiveVillainGroupCount { get; init; }
    public required int EffectiveHenchmenCount { get; init; }

    /// <summary>Scheme Twists to shuffle in, after the Scheme's own rules.</summary>
    public required int EffectiveTwists { get; init; }

    /// <summary>Bystanders in the Villain Deck, after the Scheme's own rules.</summary>
    public required int EffectiveBystanders { get; init; }
}
