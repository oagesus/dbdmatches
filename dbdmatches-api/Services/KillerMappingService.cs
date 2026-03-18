namespace DbdMatches.Api.Services;

public static class KillerMappingService
{
    private static readonly Dictionary<string, KillerInfo> KillerStatMap = new()
    {
        ["DBD_TrapPickup"] = new("The Trapper", "DBD_TrapPickup", null), //stat: grabs from traps
        ["DBD_UncloakAttack"] = new("The Wraith", "DBD_UncloakAttack", null), //stat: hits after uncloak
        ["DBD_ChainsawHit"] = new("The Hillbilly", "DBD_ChainsawHit", null), // NEEDS CHECKING (BUG)
        ["DBD_SlasherChainAttack"] = new("The Nurse", "DBD_SlasherChainAttack", null), //stat1: blink attack
        ["DBD_SlasherTierIncrement"] = new("The Shape", null, null), // NOT TRACKABLE
        ["DBD_DLC3_Slasher_Stat1"] = new("The Hag", "DBD_DLC3_Slasher_Stat1", "DBD_DLC3_Slasher_Stat2"), //stat1: traps triggered, stat2: trials where all survivors were hit after teleport
        ["DBD_DLC4_Slasher_Stat1"] = new("The Doctor", "DBD_DLC4_Slasher_Stat1", "DBD_DLC4_Slasher_Stat2"), //stat1: shocks, stat2: trials with all survivors simultaneously in tier 3 madness
        ["DBD_DLC5_Slasher_Stat1"] = new("The Huntress", "DBD_DLC5_Slasher_Stat1", "DBD_DLC5_Slasher_Stat2"), //stat1: hatchets thrown, stat2: survivors downed with a hatched (24+ meters)
        ["DBD_DLC6_Slasher_Stat1"] = new("The Cannibal", "DBD_DLC6_Slasher_Stat1", null), //stat1: chainsaw hits
        ["DBD_DLC7_Slasher_Stat1"] = new("The Nightmare", "DBD_DLC7_Slasher_Stat1", null), //stat1: survivor put to sleep
        ["DBD_DLC8_Slasher_Stat1"] = new("The Pig", "DBD_DLC8_Slasher_Stat1", null), //stat1: reverse bear-traps applied
        ["DBD_DLC9_Slasher_Stat2"] = new("The Clown", null, "DBD_DLC9_Slasher_Stat2"), //stat2: intoxicated survivors downed
        ["DBD_Chapter9_Slasher_Stat2"] = new("The Spirit", null, "DBD_Chapter9_Slasher_Stat2"), //stat2: survivors downed after phase walk
        ["DBD_Chapter10_Slasher_Stat1"] = new("The Legion", null, "DBD_Chapter10_Slasher_Stat2"), //stat2: deep-wounded survivors downed
        ["DBD_Chapter11_Slasher_Stat1"] = new("The Plague", null, "DBD_Chapter11_Slasher_Stat2"), //stat2: max sickness survivors downed
        ["DBD_Chapter12_Slasher_Stat2"] = new("The Ghost Face", null, "DBD_Chapter12_Slasher_Stat2"), //stat2: marked survivors downed
        ["DBD_Chapter13_Slasher_Stat1"] = new("The Demogorgon", null, null), // NOT TRACKABLE
        ["DBD_Chapter14_Slasher_Stat2"] = new("The Oni", null, "DBD_Chapter14_Slasher_Stat2"), //stat2: demon fury downs
        ["DBD_Chapter15_Slasher_Stat1"] = new("The Deathslinger", "DBD_Chapter15_Slasher_Stat1", null), //stat1: speared survivors downed
        ["DBD_Chapter16_Slasher_Stat1"] = new("The Executioner", "DBD_Chapter16_Slasher_Stat1", null), //stat1: survivors caged
        ["DBD_Chapter17_Slasher_Stat1"] = new("The Blight", "DBD_Chapter17_Slasher_Stat1", null), //stat1: lethal rush hits
        ["DBD_Chapter18_Slasher_Stat1"] = new("The Twins", "DBD_Chapter18_Slasher_Stat1", null), //stat1: survivors downed with victor latched on
        ["DBD_Chapter19_Slasher_Stat1"] = new("The Trickster", "DBD_Chapter19_Slasher_Stat1", null), //stat1: max laceration reached
        ["DBD_Chapter20_Slasher_Stat1"] = new("The Nemesis", "DBD_Chapter20_Slasher_Stat1", null), //stat1: contaminated survivors downed
        ["DBD_Chapter21_Slasher_Stat1"] = new("The Cenobite", null, null), // NOT TRACKABLE
        ["DBD_Chapter22_Slasher_Stat1"] = new("The Artist", "DBD_Chapter22_Slasher_Stat1", null), //stat1: survivors downed with dire crows
        ["DBD_Chapter23_Slasher_Stat1"] = new("The Onryō", "DBD_Chapter23_Slasher_Stat1", null), //stat1: survivors condemned
        ["DBD_Chapter24_Slasher_Stat1"] = new("The Dredge", "DBD_Chapter24_Slasher_Stat1", null), //stat1: survivors downed in nightfall
        ["DBD_Chapter25_Slasher_Stat1"] = new("The Mastermind", "DBD_Chapter25_Slasher_Stat1", null), //stat1: survivors slammed into another
        ["DBD_Chapter26_Slasher_Stat1"] = new("The Knight", "DBD_Chapter26_Slasher_Stat1", null), //stat1: survivors damaged while being pursued by a guard
        ["DBD_Chapter27_Slasher_Stat1"] = new("The Skull Merchant", "DBD_Chapter27_Slasher_Stat1", null), // stat1: claw trapped survivors downed
        ["DBD_Chapter28_Slasher_Stat1"] = new("The Singularity", "DBD_Chapter28_Slasher_Stat1", null, null), //stat1: survivors hit after recent teleport
        ["DBD_Chapter29_Slasher_Stat1"] = new("The Xenomorph", "DBD_Chapter29_Slasher_Stat1", null, null), //stat1: survivors hit with a tail attack
        ["DBD_Chapter30_Slasher_Stat1"] = new("The Good Guy", "DBD_Chapter30_Slasher_Stat1", null), //stat1: survivors hit within 3 seconds of a scamper
        ["DBD_Chapter31_Slasher_Stat1"] = new("The Unknown", "DBD_Chapter31_Slasher_Stat1", null), //stat1: survivors hit with a uvx blast
        ["DBD_Chapter32_Slasher_Stat1"] = new("The Lich", "DBD_Chapter32_Slasher_Stat1", null), //stat1: survivors hit within 15 seconds of casting a spell
        ["DBD_Chapter33_Slasher_Stat1"] = new("The Dark Lord", null, "DBD_Chapter33_Slasher_Stat2"), //stat2: survivors hit with hellfire
        ["DBD_Chapter34_Slasher_Stat2"] = new("The Houndmaster", null, "DBD_Chapter34_Slasher_Stat2"), //stat2: survivors injured while detained by the dog
        ["DBD_Chapter35_Slasher_Stat1"] = new("The Ghoul", "DBD_Chapter35_Slasher_Stat1", null), //stat1: perfectly timed grab-attacks
        ["DBD_Chapter36_Slasher_Stat1"] = new("The Animatronic", "DBD_Chapter36_Slasher_Stat1", null), //stat1: survivors interrupted in security office
        ["DBD_Chapter37_Slasher_Stat1"] = new("The Krasue", "DBD_Chapter37_Slasher_Stat1", null), //stat1: whip downs
        ["DBD_Chapter38_Slasher_Stat1"] = new("The First", "DBD_Chapter38_Slasher_Stat1", null), //stat1: survivors damaged with vine or undergate attacks
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
        "DBD_BloodwebPoints",
        "DBD_CamperFullLoadout",
        "DBD_CamperSkulls"
    ];

    public static readonly string[] KillerGlobalStatKeys =
    [
        "DBD_SacrificedCampers",
        "DBD_KilledCampers",
        "DBD_BloodwebPoints",
        "DBD_SlasherFullLoadout",
        "DBD_KillerSkulls"
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

    public static KillerInfo? GetByName(string killerName)
    {
        return KillerStatMap.Values.FirstOrDefault(k => k.Name == killerName);
    }

    public static List<string> GetAllKillerNames()
    {
        var killers = KillerStatMap.Values
            .Select(k => k.Name)
            .Distinct()
            .OrderBy(n => n)
            .ToList();
        killers.Add("Untracked Killer");
        return killers;
    }
}

public record KillerInfo(string Name, string? Stat1Key, string? Stat2Key, string? Stat3Key = null);
