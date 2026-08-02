using Legendary.Companion.Models;

namespace Legendary.Companion.Data;

/// <summary>
/// Content for "Legendary: A Marvel Deck Building Game — Dark City", the big-box
/// expansion (6 new Villain Groups, 2 new Henchmen Groups, 5 Masterminds,
/// 8 Schemes, 17 Heroes). Enabled by default so it can be combined with the Core
/// Set; toggle it off in Settings. Purely data — the randomiser is unchanged.
/// </summary>
public static class DarkCity
{
    private const string Id = "dark-city";

    // Villain groups referenced by Masterminds' "Always Leads" rules.
    private const string FourHorsemen = "dc:four-horsemen";
    private const string StreetsOfNewYork = "dc:streets-of-new-york";
    private const string Underworld = "dc:underworld";
    private const string Marauders = "dc:marauders";
    private const string MLF = "dc:mutant-liberation-front";

    public static readonly CardSet Set = new()
    {
        Id = Id,
        Name = "Dark City",
        Description = "Big-box expansion: X-Men, X-Force & Marvel Knights heroes vs. Apocalypse and more.",
        Released = new DateOnly(2013, 1, 1),
        Standalone = false,
        EnabledByDefault = true,

        Masterminds =
        [
            new() { Id = "dc:apocalypse", SetId = Id, Name = "Apocalypse", AlwaysLeadsGroupId = FourHorsemen, Tagline = "En Sabah Nur, the First One" },
            new() { Id = "dc:kingpin", SetId = Id, Name = "Kingpin", AlwaysLeadsGroupId = StreetsOfNewYork, Tagline = "Wilson Fisk, lord of crime" },
            new() { Id = "dc:mephisto", SetId = Id, Name = "Mephisto", AlwaysLeadsGroupId = Underworld, Tagline = "Ruler of a hell dimension" },
            new() { Id = "dc:mister-sinister", SetId = Id, Name = "Mister Sinister", AlwaysLeadsGroupId = Marauders, Tagline = "Nathaniel Essex, geneticist" },
            new() { Id = "dc:stryfe", SetId = Id, Name = "Stryfe", AlwaysLeadsGroupId = MLF, Tagline = "The Chaos-Bringer" },
        ],

        // Setup facts taken from the Scheme cards (Twist counts, forced groups,
        // count changes). Card ability wording is not reproduced.
        Schemes =
        [
            new() { Id = "dc:capture-baby-hope", SetId = Id, Name = "Capture Baby Hope", Setup = new SchemeSetup { Twists = 8, Notes = ["Place a Baby Hope token by the Mastermind."] } },
            new() { Id = "dc:detonate-helicarrier", SetId = Id, Name = "Detonate the Helicarrier", Setup = new SchemeSetup { Twists = 8, Heroes = 6, Notes = ["Play with 6 Heroes."] } },
            new() { Id = "dc:earthquake-generator", SetId = Id, Name = "Massive Earthquake Generator", Setup = new SchemeSetup { Twists = 8 } },
            new() { Id = "dc:organized-crimewave", SetId = Id, Name = "Organized Crime Wave", Setup = new SchemeSetup { Twists = 8, RequiredHenchmenGroupId = "dc:maggia-goons" } },
            new() { Id = "dc:save-humanity", SetId = Id, Name = "Save Humanity", Setup = new SchemeSetup { Twists = 8, Notes = ["Put 24 Bystanders in the Hero Deck (12 in solo)."] } },
            new() { Id = "dc:steal-plutonium", SetId = Id, Name = "Steal the Weaponized Plutonium", Setup = new SchemeSetup { Twists = 8, VillainGroupDelta = 1, Notes = ["Adds an extra Villain Group."] } },
            new() { Id = "dc:transform-demons", SetId = Id, Name = "Transform Citizens into Demons", Setup = new SchemeSetup { Twists = 8, Bystanders = 0 } },
            new() { Id = "dc:xcutioners-song", SetId = Id, Name = "X-Cutioner's Song", Setup = new SchemeSetup { Twists = 8, HeroDelta = 1, Bystanders = 0, Notes = ["Adds an extra Hero (its cards go in the Villain Deck)."] } },
        ],

        VillainGroups =
        [
            new() { Id = FourHorsemen, SetId = Id, Name = "The Four Horsemen" },
            new() { Id = Marauders, SetId = Id, Name = "Marauders" },
            new() { Id = MLF, SetId = Id, Name = "Mutant Liberation Front" },
            new() { Id = StreetsOfNewYork, SetId = Id, Name = "Streets of New York" },
            new() { Id = Underworld, SetId = Id, Name = "The Underworld" },
            new() { Id = "dc:emissaries-of-evil", SetId = Id, Name = "Emissaries of Evil" },
        ],

        Henchmen =
        [
            new() { Id = "dc:maggia-goons", SetId = Id, Name = "Maggia Goons" },
            new() { Id = "dc:phalanx", SetId = Id, Name = "Phalanx" },
        ],

        Heroes =
        [
            // X-Men
            new() { Id = "dc:angel", SetId = Id, Name = "Angel", Team = "X-Men" },
            new() { Id = "dc:bishop", SetId = Id, Name = "Bishop", Team = "X-Men" },
            new() { Id = "dc:iceman", SetId = Id, Name = "Iceman", Team = "X-Men" },
            new() { Id = "dc:jean-grey", SetId = Id, Name = "Jean Grey", Team = "X-Men" },
            new() { Id = "dc:nightcrawler", SetId = Id, Name = "Nightcrawler", Team = "X-Men" },
            new() { Id = "dc:professor-x", SetId = Id, Name = "Professor X", Team = "X-Men" },
            // X-Force
            new() { Id = "dc:cable", SetId = Id, Name = "Cable", Team = "X-Force" },
            new() { Id = "dc:colossus", SetId = Id, Name = "Colossus", Team = "X-Force" },
            new() { Id = "dc:domino", SetId = Id, Name = "Domino", Team = "X-Force" },
            new() { Id = "dc:forge", SetId = Id, Name = "Forge", Team = "X-Force" },
            new() { Id = "dc:wolverine-xforce", SetId = Id, Name = "Wolverine (X-Force)", Team = "X-Force" },
            // Marvel Knights
            new() { Id = "dc:blade", SetId = Id, Name = "Blade", Team = "Marvel Knights" },
            new() { Id = "dc:daredevil", SetId = Id, Name = "Daredevil", Team = "Marvel Knights" },
            new() { Id = "dc:elektra", SetId = Id, Name = "Elektra", Team = "Marvel Knights" },
            new() { Id = "dc:ghost-rider", SetId = Id, Name = "Ghost Rider", Team = "Marvel Knights" },
            new() { Id = "dc:iron-fist", SetId = Id, Name = "Iron Fist", Team = "Marvel Knights" },
            new() { Id = "dc:punisher", SetId = Id, Name = "Punisher", Team = "Marvel Knights" },
        ],
    };
}
