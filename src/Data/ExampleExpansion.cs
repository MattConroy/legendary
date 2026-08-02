using Legendary.Companion.Models;

namespace Legendary.Companion.Data;

/// <summary>
/// A SAMPLE expansion module used to demonstrate that sets can be toggled on/off
/// without touching the randomiser. It is disabled by default and clearly flagged
/// as example content (<see cref="CardSet.IsExample"/>) so it is never mistaken for
/// official cards.
///
/// To add a REAL expansion: copy this pattern, fill in the official cards, set
/// <c>IsExample = false</c>, and register it in <see cref="SetRegistry"/>. The
/// randomiser automatically includes it whenever the set is enabled.
/// </summary>
public static class ExampleExpansion
{
    private const string Id = "example";
    private const string SinisterSix = "example:sinister-six";

    public static readonly CardSet Set = new()
    {
        Id = Id,
        Name = "Example Expansion",
        Description = "Sample content that demonstrates enabling/disabling a set. Not official cards.",
        EnabledByDefault = false,
        IsExample = true,

        Masterminds =
        [
            new() { Id = "example:mastermind-a", SetId = Id, Name = "Example Mastermind", AlwaysLeadsGroupId = SinisterSix, Tagline = "Sample data" },
        ],
        Schemes =
        [
            new() { Id = "example:scheme-a", SetId = Id, Name = "Example Scheme", Setup = new SchemeSetup { Notes = ["Sample setup note."] } },
        ],
        VillainGroups =
        [
            new() { Id = SinisterSix, SetId = Id, Name = "Example Villains" },
        ],
        Henchmen =
        [
            new() { Id = "example:henchmen-a", SetId = Id, Name = "Example Henchmen" },
        ],
        Heroes =
        [
            new() { Id = "example:hero-a", SetId = Id, Name = "Example Hero One", Team = "Sample" },
            new() { Id = "example:hero-b", SetId = Id, Name = "Example Hero Two", Team = "Sample" },
        ],
    };
}
