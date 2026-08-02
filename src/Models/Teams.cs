namespace Legendary.Companion.Models;

/// <summary>
/// Maps a Hero's team affiliation to a small icon, used to visually distinguish
/// heroes at a glance (e.g. Wolverine on the X-Men vs. the X-Force version).
/// </summary>
public static class Teams
{
    private static readonly Dictionary<string, string> Icons = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Avengers"] = "🛡️",
        ["X-Men"] = "❌",
        ["X-Force"] = "⚔️",
        ["Marvel Knights"] = "🌙",
        ["Spider-Friends"] = "🕸️",
        ["S.H.I.E.L.D."] = "🦅",
        ["Sample"] = "🧪",
    };

    /// <summary>Icon for a team, or a neutral badge if unknown/blank.</summary>
    public static string Icon(string? team) =>
        team is not null && Icons.TryGetValue(team, out var icon) ? icon : "▪️";
}
