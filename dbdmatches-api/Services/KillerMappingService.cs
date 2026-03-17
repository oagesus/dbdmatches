namespace DbdMatches.Api.Services;

public static class KillerMappingService
{
    private static readonly Dictionary<string, KillerInfo> KillerStatMap = new()
    {
        ["DBD_TrapPickup"] = new("The Trapper", "DBD_TrapPickup", null),
        ["DBD_UncloakAttack"] = new("The Wraith", "DBD_UncloakAttack", null),
        ["DBD_ChainsawHit"] = new("The Hillbilly", "DBD_ChainsawHit", null),
        ["DBD_SlasherChainAttack"] = new("The Nurse", "DBD_SlasherChainAttack", null),
        // ["DBD_SlasherTierIncrement"] = new("The Shape", "DBD_SlasherTierIncrement", null), // global stat
        ["DBD_DLC3_Slasher_Stat1"] = new("The Hag", "DBD_DLC3_Slasher_Stat1", "DBD_DLC3_Slasher_Stat2"),
        ["DBD_DLC4_Slasher_Stat1"] = new("The Doctor", "DBD_DLC4_Slasher_Stat1", "DBD_DLC4_Slasher_Stat2"),
        ["DBD_DLC5_Slasher_Stat1"] = new("The Huntress", "DBD_DLC5_Slasher_Stat1", "DBD_DLC5_Slasher_Stat2"),
        ["DBD_DLC6_Slasher_Stat1"] = new("The Cannibal", "DBD_DLC6_Slasher_Stat1", "DBD_DLC6_Slasher_Stat2"),
        ["DBD_DLC7_Slasher_Stat1"] = new("The Nightmare", "DBD_DLC7_Slasher_Stat1", "DBD_DLC7_Slasher_Stat2"),
        ["DBD_DLC8_Slasher_Stat1"] = new("The Pig", "DBD_DLC8_Slasher_Stat1", "DBD_DLC8_Slasher_Stat2"),
        ["DBD_DLC9_Slasher_Stat2"] = new("The Clown", "DBD_DLC9_Slasher_Stat1", "DBD_DLC9_Slasher_Stat2"),
        ["DBD_Chapter9_Slasher_Stat2"] = new("The Spirit", "DBD_Chapter9_Slasher_Stat1", "DBD_Chapter9_Slasher_Stat2"),
        ["DBD_Chapter10_Slasher_Stat1"] = new("The Legion", "DBD_Chapter10_Slasher_Stat1", "DBD_Chapter10_Slasher_Stat2"),
        ["DBD_Chapter11_Slasher_Stat1"] = new("The Plague", "DBD_Chapter11_Slasher_Stat1", "DBD_Chapter11_Slasher_Stat2"),
        ["DBD_Chapter12_Slasher_Stat2"] = new("The Ghost Face", "DBD_Chapter12_Slasher_Stat1", "DBD_Chapter12_Slasher_Stat2"),
        ["DBD_Chapter13_Slasher_Stat1"] = new("The Demogorgon", "DBD_Chapter13_Slasher_Stat1", null), // Stat2 is global
        ["DBD_Chapter14_Slasher_Stat2"] = new("The Oni", "DBD_Chapter14_Slasher_Stat1", "DBD_Chapter14_Slasher_Stat2"),
        ["DBD_Chapter15_Slasher_Stat1"] = new("The Deathslinger", "DBD_Chapter15_Slasher_Stat1", "DBD_Chapter15_Slasher_Stat2"),
        ["DBD_Chapter16_Slasher_Stat1"] = new("The Executioner", "DBD_Chapter16_Slasher_Stat1", "DBD_Chapter16_Slasher_Stat2"),
        ["DBD_Chapter17_Slasher_Stat1"] = new("The Blight", "DBD_Chapter17_Slasher_Stat1", "DBD_Chapter17_Slasher_Stat2"),
        ["DBD_Chapter18_Slasher_Stat1"] = new("The Twins", "DBD_Chapter18_Slasher_Stat1", "DBD_Chapter18_Slasher_Stat2"),
        ["DBD_Chapter19_Slasher_Stat1"] = new("The Trickster", "DBD_Chapter19_Slasher_Stat1", "DBD_Chapter19_Slasher_Stat2"),
        ["DBD_Chapter20_Slasher_Stat1"] = new("The Nemesis", "DBD_Chapter20_Slasher_Stat1", "DBD_Chapter20_Slasher_Stat2"),
        ["DBD_Chapter21_Slasher_Stat1"] = new("The Cenobite", "DBD_Chapter21_Slasher_Stat1", "DBD_Chapter21_Slasher_Stat2"),
        ["DBD_Chapter22_Slasher_Stat1"] = new("The Artist", "DBD_Chapter22_Slasher_Stat1", "DBD_Chapter22_Slasher_Stat2"),
        ["DBD_Chapter23_Slasher_Stat1"] = new("The Onryō", "DBD_Chapter23_Slasher_Stat1", "DBD_Chapter23_Slasher_Stat2"),
        ["DBD_Chapter24_Slasher_Stat1"] = new("The Dredge", "DBD_Chapter24_Slasher_Stat1", "DBD_Chapter24_Slasher_Stat2"),
        ["DBD_Chapter25_Slasher_Stat1"] = new("The Mastermind", "DBD_Chapter25_Slasher_Stat1", null), // Stat2 is global
        ["DBD_Chapter26_Slasher_Stat1"] = new("The Knight", "DBD_Chapter26_Slasher_Stat1", "DBD_Chapter26_Slasher_Stat2"),
        ["DBD_Chapter27_Slasher_Stat1"] = new("The Skull Merchant", "DBD_Chapter27_Slasher_Stat1", null), // Stat2 is global
        ["DBD_Chapter28_Slasher_Stat1"] = new("The Singularity", "DBD_Chapter28_Slasher_Stat1", "DBD_Chapter28_Slasher_Stat2", "DBD_Chapter28_Slasher_Stat3"),
        ["DBD_Chapter29_Slasher_Stat1"] = new("The Xenomorph", "DBD_Chapter29_Slasher_Stat1", "DBD_Chapter29_Slasher_Stat2", "DBD_Chapter29_Slasher_Stat3"),
        ["DBD_Chapter30_Slasher_Stat1"] = new("The Good Guy", "DBD_Chapter30_Slasher_Stat1", null),
        ["DBD_Chapter31_Slasher_Stat1"] = new("The Unknown", "DBD_Chapter31_Slasher_Stat1", null),
        ["DBD_Chapter32_Slasher_Stat1"] = new("The Lich", "DBD_Chapter32_Slasher_Stat1", "DBD_Chapter32_Slasher_Stat2"),
        ["DBD_Chapter33_Slasher_Stat1"] = new("The Dark Lord", "DBD_Chapter33_Slasher_Stat1", "DBD_Chapter33_Slasher_Stat2"),
        // ["DBD_Chapter34_Slasher_Stat1"] = new("The Houndmaster", ...), // Stat1 is global
        ["DBD_Chapter34_Slasher_Stat2"] = new("The Houndmaster", "DBD_Chapter34_Slasher_Stat1", "DBD_Chapter34_Slasher_Stat2"),
        ["DBD_Chapter35_Slasher_Stat1"] = new("The Ghoul", "DBD_Chapter35_Slasher_Stat1", null),
        ["DBD_Chapter36_Slasher_Stat1"] = new("The Animatronic", "DBD_Chapter36_Slasher_Stat1", "DBD_Chapter36_Slasher_Stat2"),
        ["DBD_Chapter37_Slasher_Stat1"] = new("The Krasue", "DBD_Chapter37_Slasher_Stat1", null),
        ["DBD_Chapter38_Slasher_Stat1"] = new("The First", "DBD_Chapter38_Slasher_Stat1", null),
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
        return KillerStatMap.Values
            .Select(k => k.Name)
            .Distinct()
            .OrderBy(n => n)
            .ToList();
    }
}

public record KillerInfo(string Name, string Stat1Key, string? Stat2Key, string? Stat3Key = null);
