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
            new()
            {
                Id = "core:legacy-virus", SetId = Id, Name = "The Legacy Virus",
                Setup = new SchemeSetup { Notes = ["Set aside the Wounds as normal; the Legacy Virus twists spread Wounds — keep the Wound stack handy."] },
            },
            new()
            {
                Id = "core:midtown-bank", SetId = Id, Name = "Midtown Bank Robbery",
                Setup = new SchemeSetup { Notes = ["Add extra Bystanders to the Villain Deck (see Scheme card)."] },
            },
            new()
            {
                Id = "core:negative-zone", SetId = Id, Name = "Negative Zone Prison Breakout",
                Setup = new SchemeSetup { Notes = ["Stack the Master Strikes as directed by the Scheme; escaping Villains raise the stakes."] },
            },
            new()
            {
                Id = "core:dark-dimension", SetId = Id, Name = "Portals to the Dark Dimension",
                Setup = new SchemeSetup { Notes = ["Place the Dark Portal twists per the Scheme card."] },
            },
            new()
            {
                Id = "core:killbots", SetId = Id, Name = "Replace Earth's Leaders with Killbots",
                // Official setup: add an extra Henchman Group to the Villain Deck.
                Setup = new SchemeSetup
                {
                    HenchmenDelta = 1,
                    Notes = ["Add one extra Henchman Group to the Villain Deck (already included in the count above)."],
                },
            },
            new()
            {
                Id = "core:secret-invasion", SetId = Id, Name = "Secret Invasion of the Skrull Shapeshifters",
                Setup = new SchemeSetup { Notes = ["Skrull Scheme Twists impersonate Heroes — resolve per the Scheme card."] },
            },
            new()
            {
                Id = "core:civil-war", SetId = Id, Name = "Super Hero Civil War",
                // Official setup: play with an extra Hero in the HQ.
                Setup = new SchemeSetup
                {
                    HeroDelta = 1,
                    Notes = ["Play with one extra Hero in the Hero Deck / HQ (already included in the count above)."],
                },
            },
            new()
            {
                Id = "core:cosmic-cube", SetId = Id, Name = "Unleash the Power of the Cosmic Cube",
                Setup = new SchemeSetup { Notes = ["Add extra Scheme Twists to the Villain Deck per the Scheme card."] },
            },
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
