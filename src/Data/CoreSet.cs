using Legendary.Companion.Models;

namespace Legendary.Companion.Data;

/// <summary>
/// Content for "Legendary: A Marvel Deck Building Game" — the Core Set.
/// This is a plain data module: adding an expansion means writing another file
/// like this one and registering it in <see cref="SetRegistry"/>. The randomiser
/// never references it directly.
/// </summary>
public static class CoreSet
{
    private const string Id = "core";

    // Group ids referenced by Masterminds' "Always Leads" rules.
    private const string Brotherhood = "core:brotherhood";
    private const string EnemiesOfAsgard = "core:enemies-of-asgard";
    private const string Hydra = "core:hydra";
    private const string DoombotLegion = "core:doombot-legion";

    // Every base Scheme has special setup/twist rules printed on its card. We don't
    // reproduce that text (see About → "Why no full card text"), so each simply
    // reminds the player to read the card. No count deltas are asserted.
    private static readonly SchemeSetup Special = new()
    {
        Notes = ["This Scheme has special setup and Twist rules — follow the text on the Scheme card."],
    };

    public static readonly CardSet Set = new()
    {
        Id = Id,
        Name = "Core Set",
        Description = "Legendary: A Marvel Deck Building Game — the original base game.",
        EnabledByDefault = true,

        Masterminds =
        [
            new() { Id = "core:dr-doom", SetId = Id, Name = "Dr. Doom", AlwaysLeadsGroupId = DoombotLegion, Tagline = "Ruler of Latveria" },
            new() { Id = "core:loki", SetId = Id, Name = "Loki", AlwaysLeadsGroupId = EnemiesOfAsgard, Tagline = "God of Mischief" },
            new() { Id = "core:magneto", SetId = Id, Name = "Magneto", AlwaysLeadsGroupId = Brotherhood, Tagline = "Master of Magnetism" },
            new() { Id = "core:red-skull", SetId = Id, Name = "Red Skull", AlwaysLeadsGroupId = Hydra, Tagline = "Leader of HYDRA" },
        ],

        Schemes =
        [
            // Notes deliberately point players to the Scheme card rather than
            // restating its exact setup/twist text — see the About page for why.
            new() { Id = "core:legacy-virus", SetId = Id, Name = "The Legacy Virus", Setup = Special },
            new() { Id = "core:midtown-bank", SetId = Id, Name = "Midtown Bank Robbery", Setup = Special },
            new() { Id = "core:negative-zone", SetId = Id, Name = "Negative Zone Prison Breakout", Setup = Special },
            new() { Id = "core:dark-dimension", SetId = Id, Name = "Portals to the Dark Dimension", Setup = Special },
            new() { Id = "core:killbots", SetId = Id, Name = "Replace Earth's Leaders with Killbots", Setup = Special },
            new() { Id = "core:secret-invasion", SetId = Id, Name = "Secret Invasion of the Skrull Shapeshifters", Setup = Special },
            new() { Id = "core:civil-war", SetId = Id, Name = "Super Hero Civil War", Setup = Special },
            new() { Id = "core:cosmic-cube", SetId = Id, Name = "Unleash the Power of the Cosmic Cube", Setup = Special },
        ],

        VillainGroups =
        [
            new() { Id = Brotherhood, SetId = Id, Name = "The Brotherhood" },
            new() { Id = EnemiesOfAsgard, SetId = Id, Name = "The Enemies of Asgard" },
            new() { Id = Hydra, SetId = Id, Name = "HYDRA" },
            new() { Id = "core:skrulls", SetId = Id, Name = "Skrulls" },
            new() { Id = "core:spider-foes", SetId = Id, Name = "Spider-Foes" },
            new() { Id = "core:radiation", SetId = Id, Name = "Radiation" },
        ],

        Henchmen =
        [
            new() { Id = DoombotLegion, SetId = Id, Name = "Doombot Legion" },
            new() { Id = "core:hand-ninjas", SetId = Id, Name = "Hand Ninjas" },
            new() { Id = "core:savage-land-mutates", SetId = Id, Name = "Savage Land Mutates" },
            new() { Id = "core:sentinels", SetId = Id, Name = "Sentinels" },
        ],

        Heroes =
        [
            new() { Id = "core:black-widow", SetId = Id, Name = "Black Widow", Team = "Avengers" },
            new() { Id = "core:captain-america", SetId = Id, Name = "Captain America", Team = "Avengers" },
            new() { Id = "core:cyclops", SetId = Id, Name = "Cyclops", Team = "X-Men" },
            new() { Id = "core:deadpool", SetId = Id, Name = "Deadpool", Team = "X-Force" },
            new() { Id = "core:emma-frost", SetId = Id, Name = "Emma Frost", Team = "X-Men" },
            new() { Id = "core:gambit", SetId = Id, Name = "Gambit", Team = "X-Men" },
            new() { Id = "core:hawkeye", SetId = Id, Name = "Hawkeye", Team = "Avengers" },
            new() { Id = "core:hulk", SetId = Id, Name = "Hulk", Team = "Avengers" },
            new() { Id = "core:iron-man", SetId = Id, Name = "Iron Man", Team = "Avengers" },
            new() { Id = "core:nick-fury", SetId = Id, Name = "Nick Fury", Team = "S.H.I.E.L.D." },
            new() { Id = "core:rogue", SetId = Id, Name = "Rogue", Team = "X-Men" },
            new() { Id = "core:spider-man", SetId = Id, Name = "Spider-Man", Team = "Spider-Friends" },
            new() { Id = "core:storm", SetId = Id, Name = "Storm", Team = "X-Men" },
            new() { Id = "core:thor", SetId = Id, Name = "Thor", Team = "Avengers" },
            new() { Id = "core:wolverine", SetId = Id, Name = "Wolverine", Team = "X-Men" },
        ],
    };
}
