namespace Legendary.Companion.Models;

/// <summary>Coarse difficulty band a Threat score falls into.</summary>
public enum DifficultyBand { Easy, Medium, Hard }

/// <summary>
/// Turns the editorial difficulty ratings on a setup into a single Threat score
/// out of 10. The Mastermind is the anchor — the most reliable signal — setting a
/// base of <c>rating × 2 − 1</c> (1, 3, 5, 7, 9). The Scheme is a small ±1 modifier
/// (its rating relative to an average of 3), since scheme difficulty is contextual.
/// Villain groups can fold in later as another small modifier. Result clamps to 1–10.
/// </summary>
public static class Threat
{
    /// <summary>Threat out of 10, or null if the Mastermind isn't rated.</summary>
    public static int? Score(GameSetup setup)
    {
        if ((setup.Mastermind.Card as Mastermind)?.Difficulty is not { } mastermind)
            return null;
        return Score(mastermind, (setup.Scheme.Card as Scheme)?.Difficulty);
    }

    /// <summary>Pure scoring: Mastermind base + small Scheme modifier, clamped 1–10.</summary>
    public static int Score(int mastermind, int? scheme)
    {
        var baseline = mastermind * 2 - 1;
        var modifier = scheme is { } s ? Math.Clamp(s - 3, -1, 1) : 0;
        return Math.Clamp(baseline + modifier, 1, 10);
    }

    /// <summary>Band for a score: 1–3 Easy, 4–7 Medium, 8–10 Hard.</summary>
    public static DifficultyBand Band(int score) =>
        score <= 3 ? DifficultyBand.Easy : score <= 7 ? DifficultyBand.Medium : DifficultyBand.Hard;
}
