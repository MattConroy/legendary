using Legendary.Companion.Models;

namespace Legendary.Companion.Data;

/// <summary>
/// Content for "Legendary: A Marvel Deck Building Game — Fantastic Four", the first
/// small-box expansion (2 Masterminds, 4 Schemes, 2 Villain Groups, 5 Heroes; no
/// Henchmen). Requires a Core set to play. Setup facts are taken from the cards.
/// </summary>
public static class FantasticFour
{
    private const string Id = "fantastic-four";
    private const string HeraldsOfGalactus = "ff:heralds-of-galactus";
    private const string Subterranea = "ff:subterranea";

    public static readonly CardSet Set = new()
    {
        Id = Id,
        Name = "Fantastic Four",
        Description = "First small-box expansion: the Fantastic Four & Silver Surfer vs. Galactus and Mole Man.",
        Released = new DateOnly(2015, 1, 1),
        Standalone = false,
        EnabledByDefault = false,

        Masterminds =
        [
            new() { Id = "ff:galactus", SetId = Id, Name = "Galactus", AlwaysLeadsGroupId = HeraldsOfGalactus, Tagline = "Devourer of Worlds" },
            new() { Id = "ff:mole-man", SetId = Id, Name = "Mole Man", AlwaysLeadsGroupId = Subterranea, Tagline = "Monarch of Subterranea" },
        ],

        Schemes =
        [
            new() { Id = "ff:cosmic-rays", SetId = Id, Name = "Bathe the Earth in Cosmic Rays", Setup = new SchemeSetup { Twists = 6 } },
            new() { Id = "ff:melted-glaciers", SetId = Id, Name = "Flood the Planet with Melted Glaciers", Setup = new SchemeSetup { Twists = 8 } },
            new() { Id = "ff:force-field", SetId = Id, Name = "Invincible Force Field", Setup = new SchemeSetup { Twists = 7 } },
            new() { Id = "ff:negative-zone", SetId = Id, Name = "Pull Reality Into the Negative Zone", Setup = new SchemeSetup { Twists = 8 } },
        ],

        VillainGroups =
        [
            new() { Id = HeraldsOfGalactus, SetId = Id, Name = "Heralds of Galactus" },
            new() { Id = Subterranea, SetId = Id, Name = "Subterranea" },
        ],

        // Fantastic Four adds no Henchman groups.
        Henchmen = [],

        Heroes =
        [
            new() { Id = "ff:mister-fantastic", SetId = Id, Name = "Mister Fantastic", Team = "Fantastic Four" },
            new() { Id = "ff:invisible-woman", SetId = Id, Name = "Invisible Woman", Team = "Fantastic Four" },
            new() { Id = "ff:human-torch", SetId = Id, Name = "Human Torch", Team = "Fantastic Four" },
            new() { Id = "ff:thing", SetId = Id, Name = "Thing", Team = "Fantastic Four" },
            new() { Id = "ff:silver-surfer", SetId = Id, Name = "Silver Surfer", Team = "Unaffiliated" },
        ],
    };
}
