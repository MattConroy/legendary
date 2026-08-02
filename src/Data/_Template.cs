using Legendary.Companion.Models;

namespace Legendary.Companion.Data;

// ─────────────────────────────────────────────────────────────────────────────
// NEW-SET TEMPLATE  (excluded from the build — see the .csproj <Compile Remove>)
//
// To add an expansion:
//   1. Copy this file to Data/YourSet.cs and rename the class.
//   2. Remove the <Compile Remove="Data\_Template.cs" /> line's protection by
//      simply using a real filename (only _Template.cs is excluded).
//   3. Fill in the data from the card text, then register it in SetRegistry.cs.
//
// ⚠️ ID STABILITY IS A PERMANENT CONTRACT.
//   Set ids and card ids are used as local-storage keys for a player's owned /
//   in-play selections. NEVER rename or reuse an id once it has shipped, or you
//   will silently wipe people's saved collections. Pick a stable slug and keep
//   it forever. Fixing a typo in a *display Name* is fine; changing an *Id* is not.
//
//   Convention: set id = kebab-case slug ("dark-city"); card id = "<setId>:<slug>".
// ─────────────────────────────────────────────────────────────────────────────

public static class TemplateSet
{
    private const string Id = "your-set-id"; // permanent, unique, kebab-case

    // Give ids to any group a Mastermind "Always Leads" so they can be referenced.
    private const string SomeVillainGroup = "your-set-id:some-villains";

    public static readonly CardSet Set = new()
    {
        Id = Id,
        Name = "Your Set Name",
        Description = "One-line blurb (not shown in the streamlined list).",
        Released = new DateOnly(2020, 1, 1), // month/day may be 1/1 if only the year is known
        Standalone = false,                  // true for a big box playable on its own
        EnabledByDefault = false,            // shipped sets beyond the defaults start off

        Masterminds =
        [
            new() { Id = $"{Id}:example-mastermind", SetId = Id, Name = "Example Mastermind", AlwaysLeadsGroupId = SomeVillainGroup, Tagline = "Optional flavour" },
        ],

        // Only model FACTS from the card: Twists (+ per-player overrides), count
        // changes, forced groups, bystander overrides. Never the ability wording.
        Schemes =
        [
            new() { Id = $"{Id}:example-scheme", SetId = Id, Name = "Example Scheme", Setup = new SchemeSetup { Twists = 8, Notes = ["Short factual setup note."] } },
        ],

        VillainGroups =
        [
            new() { Id = SomeVillainGroup, SetId = Id, Name = "Some Villains" },
        ],

        Henchmen =
        [
            new() { Id = $"{Id}:example-henchmen", SetId = Id, Name = "Example Henchmen" },
        ],

        Heroes =
        [
            new() { Id = $"{Id}:example-hero", SetId = Id, Name = "Example Hero", Team = "Avengers" },
        ],
    };
}
