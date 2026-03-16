namespace DbdMatches.Api.Services;

public static class KillerMappingService
{
    private static readonly Dictionary<string, KillerInfo> KillerStatMap = new()
    {
        ["DBD_TrapPickup"] = new("The Trapper", "DBD_TrapPickup", null, "Bear Trap catches", null),
        ["DBD_UncloakAttack"] = new("The Wraith", "DBD_UncloakAttack", null, "Uncloak attacks", null),
        ["DBD_ChainsawHit"] = new("The Hillbilly", "DBD_ChainsawHit", null, "Chainsaw hits", null),
        ["DBD_SlasherChainAttack"] = new("The Nurse", "DBD_SlasherChainAttack", null, "Blink attacks", null),
        ["DBD_SlasherTierIncrement"] = new("The Shape", "DBD_SlasherTierIncrement", null, "Evil Within tier ups", null), //need checking
        ["DBD_DLC3_Slasher_Stat1"] = new("The Hag", "DBD_DLC3_Slasher_Stat1", "DBD_DLC3_Slasher_Stat2", "Phantasms triggered", "Phantom trap hits"),
        ["DBD_DLC4_Slasher_Stat1"] = new("The Doctor", "DBD_DLC4_Slasher_Stat1", "DBD_DLC4_Slasher_Stat2", "Shocks", "Trials with all survivors in madness 3"),
        ["DBD_DLC5_Slasher_Stat1"] = new("The Huntress", "DBD_DLC5_Slasher_Stat1", "DBD_DLC5_Slasher_Stat2", "Hatchets thrown", "Survivors downed with a Hatchet (24+ meters)"),
        ["DBD_DLC6_Slasher_Stat1"] = new("The Cannibal", "DBD_DLC6_Slasher_Stat1", "DBD_DLC6_Slasher_Stat2", "Downed survivors with chainsaw", "Survivors hooked in basement"),
        ["DBD_DLC7_Slasher_Stat1"] = new("The Nightmare", "DBD_DLC7_Slasher_Stat1", "DBD_DLC7_Slasher_Stat2", "Survivors put to dream state", "Obsessions sacrificed"),
        ["DBD_DLC8_Slasher_Stat1"] = new("The Pig", "DBD_DLC8_Slasher_Stat1", "DBD_DLC8_Slasher_Stat2", "Reverse bear traps placed", "Survivors killed after all gens repaired"),
        ["DBD_DLC9_Slasher_Stat1"] = new("The Clown", "DBD_DLC9_Slasher_Stat1", "DBD_DLC9_Slasher_Stat2", "Generators damaged with survivor hooked", "Survivors downed while intoxicated"),
        ["DBD_Chapter9_Slasher_Stat1"] = new("The Spirit", "DBD_Chapter9_Slasher_Stat1", "DBD_Chapter9_Slasher_Stat2", "Pallet chase hits", "Survivors downed after haunting"),
        ["DBD_Chapter10_Slasher_Stat1"] = new("The Legion", "DBD_Chapter10_Slasher_Stat1", "DBD_Chapter10_Slasher_Stat2", "Frenzy hits", "Survivors downed while in Deep Wound"),
        ["DBD_Chapter11_Slasher_Stat1"] = new("The Plague", "DBD_Chapter11_Slasher_Stat1", "DBD_Chapter11_Slasher_Stat2", "Vile purge infections", "Survivors downed while having max sickness"),
        ["DBD_Chapter12_Slasher_Stat1"] = new("The Ghost Face", "DBD_Chapter12_Slasher_Stat1", "DBD_Chapter12_Slasher_Stat2", "Generator grabs", "Survivors downed while marked"),
        ["DBD_Chapter13_Slasher_Stat1"] = new("The Demogorgon", "DBD_Chapter13_Slasher_Stat1", "DBD_Chapter13_Slasher_Stat2", "Shred hits", "Portal traversals"),
        ["DBD_Chapter14_Slasher_Stat1"] = new("The Oni", "DBD_Chapter14_Slasher_Stat1", "DBD_Chapter14_Slasher_Stat2", "Injured hook hits", "Survivors downed while using Blood Fury"),
        ["DBD_Chapter15_Slasher_Stat1"] = new("The Deathslinger", "DBD_Chapter15_Slasher_Stat1", "DBD_Chapter15_Slasher_Stat2", "Survivors downed while speared", "Totem grabs"),
        ["DBD_Chapter16_Slasher_Stat1"] = new("The Executioner", "DBD_Chapter16_Slasher_Stat1", "DBD_Chapter16_Slasher_Stat2", "Survivors sent to Cages of Atonement", "Rites of Judgment hits"),
        ["DBD_Chapter17_Slasher_Stat1"] = new("The Blight", "DBD_Chapter17_Slasher_Stat1", "DBD_Chapter17_Slasher_Stat2", "Lethal rush hits", "Rush tokens used"),
        ["DBD_Chapter18_Slasher_Stat1"] = new("The Twins", "DBD_Chapter18_Slasher_Stat1", "DBD_Chapter18_Slasher_Stat2", "Survivors downed while Victor is clinging", "Victor pounces"),
        ["DBD_Chapter19_Slasher_Stat1"] = new("The Trickster", "DBD_Chapter19_Slasher_Stat1", "DBD_Chapter19_Slasher_Stat2", "Max lacerations", "Knives hit"),
        ["DBD_Chapter20_Slasher_Stat1"] = new("The Nemesis", "DBD_Chapter20_Slasher_Stat1", "DBD_Chapter20_Slasher_Stat2", "Survivors downed while contaminated", "Zombie hits"),
        ["DBD_Chapter21_Slasher_Stat1"] = new("The Cenobite", "DBD_Chapter21_Slasher_Stat1", "DBD_Chapter21_Slasher_Stat2", "Chain hits", "Lament Config picks"),
        ["DBD_Chapter22_Slasher_Stat1"] = new("The Artist", "DBD_Chapter22_Slasher_Stat1", "DBD_Chapter22_Slasher_Stat2", "Survivors downed with Dire Crows", "Ink hits"),
        ["DBD_Chapter23_Slasher_Stat1"] = new("The Onryō", "DBD_Chapter23_Slasher_Stat1", "DBD_Chapter23_Slasher_Stat2", "Condemned survivors", "Condemn progress"),
        ["DBD_Chapter24_Slasher_Stat1"] = new("The Dredge", "DBD_Chapter24_Slasher_Stat1", "DBD_Chapter24_Slasher_Stat2", "Survivors downed during Nightfall", "Locker teleports"),
        ["DBD_Chapter25_Slasher_Stat1"] = new("The Mastermind", "DBD_Chapter25_Slasher_Stat1", "DBD_Chapter25_Slasher_Stat2", "Survivors slammed into another", "Bound hits"),
        ["DBD_Chapter26_Slasher_Stat1"] = new("The Knight", "DBD_Chapter26_Slasher_Stat1", "DBD_Chapter26_Slasher_Stat2", "Survivors damaged while actively pursued by a Guard", "Patrol paths"), //need checking
        ["DBD_Chapter27_Slasher_Stat1"] = new("The Skull Merchant", "DBD_Chapter27_Slasher_Stat1", "DBD_Chapter27_Slasher_Stat2", "Survivors downed while exposed by Lock On", "Drone deployments"), //need checking
        ["DBD_Chapter28_Slasher_Stat1"] = new("The Singularity", "DBD_Chapter28_Slasher_Stat1", "DBD_Chapter28_Slasher_Stat2", "Survivors hit after teleporting", "Biopod tags", "DBD_Chapter28_Slasher_Stat3", "Overclock hits"), //need checking
        ["DBD_Chapter29_Slasher_Stat1"] = new("The Xenomorph", "DBD_Chapter29_Slasher_Stat1", "DBD_Chapter29_Slasher_Stat2", "Survivors hit with Tail Attacks", "Tunnel uses", "DBD_Chapter29_Slasher_Stat3", "Runner mode hits"), //need checking
        ["DBD_Chapter30_Slasher_Stat1"] = new("The Good Guy", "DBD_Chapter30_Slasher_Stat1", null, "Survivors hit within 3 seconds of performing a Scamper", null),
        ["DBD_Chapter31_Slasher_Stat1"] = new("The Unknown", "DBD_Chapter31_Slasher_Stat1", null, "Survivors downed using UVX", null),
        ["DBD_Chapter32_Slasher_Stat1"] = new("The Lich", "DBD_Chapter32_Slasher_Stat1", "DBD_Chapter32_Slasher_Stat2", "Survivors hit within 15 seconds of casting a spell", "Survivors killed or sacrificed using Hand or Eye of Vecna"),
        ["DBD_Chapter33_Slasher_Stat1"] = new("The Dark Lord", "DBD_Chapter33_Slasher_Stat1", "DBD_Chapter33_Slasher_Stat2", "Form transformations", "Survivors hit using Hellfire"), //need checking!
        ["DBD_Chapter34_Slasher_Stat1"] = new("The Houndmaster", "DBD_Chapter34_Slasher_Stat1", "DBD_Chapter34_Slasher_Stat2", "Dog commands", "Survivors injured while detained by the Dog"), //need checking
        ["DBD_Chapter35_Slasher_Stat1"] = new("The Ghoul", "DBD_Chapter35_Slasher_Stat1", null, "Survivors hit using grab-attack", null),
        ["DBD_Chapter36_Slasher_Stat1"] = new("The Animatronic", "DBD_Chapter36_Slasher_Stat1", "DBD_Chapter36_Slasher_Stat2", "Survivors interrupted in Security Office", "Survivors hit while Oblivious"), //need checking
        ["DBD_Chapter37_Slasher_Stat1"] = new("The Krasue", "DBD_Chapter37_Slasher_Stat1", null, "Survivors downed with Intestinal Whip", null),
        ["DBD_Chapter38_Slasher_Stat1"] = new("The First", "DBD_Chapter38_Slasher_Stat1", null, "Vine or Undergate Attack hits", null),
    };

    private static readonly HashSet<string> KillerStat2Keys = KillerStatMap.Values
        .Where(k => k.Stat2Key != null)
        .Select(k => k.Stat2Key!)
        .ToHashSet();

    private static readonly HashSet<string> KillerStat3Keys = KillerStatMap.Values
        .Where(k => k.Stat3Key != null)
        .Select(k => k.Stat3Key!)
        .ToHashSet();

    public static readonly HashSet<string> AllKillerStatKeys = KillerStatMap.Keys
        .Union(KillerStat2Keys)
        .Union(KillerStat3Keys)
        .ToHashSet();

    public static readonly string[] SurvivorStatKeys =
    [
        "DBD_Escape",
        "DBD_EscapeThroughHatch",
        "DBD_GeneratorPct_float",
        "DBD_UnhookOrHeal",
        "DBD_BloodwebPoints"
    ];

    public static readonly string[] KillerGlobalStatKeys =
    [
        "DBD_SacrificedCampers",
        "DBD_KilledCampers",
        "DBD_BloodwebPoints"
    ];

    public static readonly string[] AllTrackedStatKeys = SurvivorStatKeys
        .Union(KillerGlobalStatKeys)
        .Union(AllKillerStatKeys)
        .Distinct()
        .ToArray();

    public static KillerInfo? IdentifyKiller(Dictionary<string, double> statDeltas)
    {
        foreach (var (statKey, killer) in KillerStatMap)
        {
            if (statDeltas.TryGetValue(statKey, out var delta) && delta > 0)
                return killer;
        }

        return null;
    }

    public static int GetPowerStat1Delta(KillerInfo killer, Dictionary<string, double> statDeltas)
    {
        return statDeltas.TryGetValue(killer.Stat1Key, out var delta) ? (int)delta : 0;
    }

    public static int GetPowerStat2Delta(KillerInfo killer, Dictionary<string, double> statDeltas)
    {
        if (killer.Stat2Key == null)
            return 0;

        return statDeltas.TryGetValue(killer.Stat2Key, out var delta) ? (int)delta : 0;
    }

    public static int GetPowerStat3Delta(KillerInfo killer, Dictionary<string, double> statDeltas)
    {
        if (killer.Stat3Key == null)
            return 0;

        return statDeltas.TryGetValue(killer.Stat3Key, out var delta) ? (int)delta : 0;
    }

    public static KillerInfo? GetByName(string killerName)
    {
        return KillerStatMap.Values.FirstOrDefault(k => k.Name == killerName);
    }

    public static List<string> GetAllKillerNames()
    {
        return KillerStatMap.Values
            .Select(k => k.Name)
            .Distinct()
            .OrderBy(n => n)
            .ToList();
    }
}

public record KillerInfo(string Name, string Stat1Key, string? Stat2Key, string Stat1Label, string? Stat2Label, string? Stat3Key = null, string? Stat3Label = null);
