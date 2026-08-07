namespace Legendary.Companion.Models;

/// <summary>Coarse difficulty band a Threat score falls into.</summary>
public enum DifficultyBand { Easy, Medium, Hard }

/// <summary>
/// Turns the editorial 1–5 difficulty ratings on a setup's cards into a single
/// Threat score out of 10. Score = average of the rated components × 2, so more
/// components (villains later) fold in without changing the /10 scale. Today the
/// rated components are the Mastermind and the Scheme.
/// </summary>
public static class Threat
{
    /// <summary>Threat out of 10, or null if nothing in the setup is rated yet.</summary>
    public static int? Score(GameSetup setup)
    {
        var ratings = Ratings(setup).ToList();
        if (ratings.Count == 0) return null;
        return (int)Math.Round(ratings.Average() * 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Band for a score. The score runs 2–10 (two 1–5 components), so the bands are
    /// equal thirds of that range — 2–4 Easy, 5–7 Medium, 8–10 Hard — which keeps
    /// every band reachable (a genuinely easy game needs both components low).
    /// </summary>
    public static DifficultyBand Band(int score) =>
        score <= 4 ? DifficultyBand.Easy : score <= 7 ? DifficultyBand.Medium : DifficultyBand.Hard;

    private static IEnumerable<int> Ratings(GameSetup setup)
    {
        if ((setup.Mastermind.Card as Mastermind)?.Difficulty is { } m) yield return m;
        if ((setup.Scheme.Card as Scheme)?.Difficulty is { } s) yield return s;
    }
}
