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

    /// <summary>Every card this roll put on the table, across all categories.</summary>
    public IEnumerable<GameCard> AllCards
    {
        get
        {
            yield return Mastermind.Card;
            yield return Scheme.Card;
            foreach (var v in VillainGroups) yield return v.Card;
            foreach (var h in Henchmen) yield return h.Card;
            foreach (var h in Heroes) yield return h.Card;
        }
    }

    /// <summary>Ids of every card on the table — e.g. to look up their keywords.</summary>
    public IEnumerable<string> AllCardIds => AllCards.Select(c => c.Id);

    /// <summary>
    /// This setup's overall Threat, or null when the Mastermind is unrated. The
    /// Mastermind sets the base and the Scheme applies a small ±1 modifier.
    /// </summary>
    public Threat? Threat =>
        Mastermind.Card is Mastermind { ThreatBase: { } baseline }
            ? Models.Threat.From(baseline, (Scheme.Card as Scheme)?.ThreatModifier)
            : null;
}
