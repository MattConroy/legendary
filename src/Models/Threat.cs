namespace Legendary.Companion.Models;

/// <summary>Coarse difficulty band a Threat score falls into.</summary>
public enum DifficultyBand { Easy, Medium, Hard }

/// <summary>
/// The Threat rating of a setup: a single score out of 10 and the band it falls
/// in. It is built from a Mastermind's base contribution and a Scheme's small
/// modifier — see <see cref="Mastermind.ThreatBase"/> and
/// <see cref="Scheme.ThreatModifier"/>. The Mastermind anchors the score (the most
/// reliable signal); the Scheme only nudges it ±1. A setup exposes its own Threat
/// via <see cref="GameSetup.Threat"/>.
/// </summary>
public readonly record struct Threat
{
    public const int Min = 1;
    public const int Max = 10;

    public Threat(int score) => Score = Math.Clamp(score, Min, Max);

    /// <summary>Threat out of 10 (clamped to 1–10).</summary>
    public int Score { get; }

    /// <summary>Band for the score: 1–3 Easy, 4–7 Medium, 8–10 Hard.</summary>
    public DifficultyBand Band =>
        Score <= 3 ? DifficultyBand.Easy : Score <= 7 ? DifficultyBand.Medium : DifficultyBand.Hard;

    /// <summary>Combine a Mastermind base with an optional Scheme modifier.</summary>
    public static Threat From(int mastermindBase, int? schemeModifier) =>
        new(mastermindBase + (schemeModifier ?? 0));
}
