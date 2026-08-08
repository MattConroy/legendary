namespace Legendary.Companion.Models;

/// <summary>
/// One card type within a Hero's (or group's) deck: its name, how many copies are
/// in the deck, and its factual stats. Only non-copyrightable facts are modelled —
/// the card's ability wording is not reproduced here; the app links out for that.
/// </summary>
public sealed record CardDetail
{
    public required string Name { get; init; }

    /// <summary>How many copies of this card the deck contains.</summary>
    public required int Copies { get; init; }

    /// <summary>Hero Class: Covert, Instinct, Ranged, Strength or Tech. Null for the
    /// class-less starter cards (and any card whose class isn't recorded).</summary>
    public string? Class { get; init; }

    /// <summary>The resource this card provides: "attack", "recruit", or null (effect-only).</summary>
    public string? Kind { get; init; }

    /// <summary>The printed Attack/Recruit value, when the card provides one.</summary>
    public int? Value { get; init; }

    /// <summary>True when the value is a "+" baseline that grows (e.g. "0+ Attack").</summary>
    public bool Variable { get; init; }

    /// <summary>Recruit cost to buy the card, when it has one.</summary>
    public int? Cost { get; init; }

    /// <summary>Ability keyword tokens printed on the card, with their values where
    /// given (e.g. "Versatile 2", "Teleport"). Empty if the card has none.</summary>
    public IReadOnlyList<string> Keywords { get; init; } = [];
}
