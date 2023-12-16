namespace NebulaModel.Packets.Session;

public class LobbyUpdateValues
{
    public LobbyUpdateValues() { }

    public LobbyUpdateValues(int galaxyAlgo, int galaxySeed, int starCount, float resourceMultiplier, bool isSandboxMode, bool isPeaceMode, CombatSettings combatSettings)
    {
        GalaxyAlgo = galaxyAlgo;
        GalaxySeed = galaxySeed;
        StarCount = starCount;
        ResourceMultiplier = resourceMultiplier;
        IsSandboxMode = isSandboxMode;
        IsPeaceMode = isPeaceMode;
        CombatAggressiveness = combatSettings.aggressiveness;
        CombatInitialLevel = combatSettings.initialLevel;
        CombatInitialGrowth = combatSettings.initialGrowth;
        CombatInitialColonize = combatSettings.initialColonize;
        CombatMaxDensity = combatSettings.maxDensity;
        CombatGrowthSpeedFactor = combatSettings.growthSpeedFactor;
        CombatPowerThreatFactor = combatSettings.powerThreatFactor;
        CombatBattleThreatFactor = combatSettings.battleThreatFactor;
        CombatBattleExpFactor = combatSettings.battleExpFactor;
    }

    public int GalaxyAlgo { get; set; }
    public int GalaxySeed { get; set; }
    public int StarCount { get; set; }
    public float ResourceMultiplier { get; set; }
    public bool IsSandboxMode { get; set; }
    public bool IsPeaceMode { get; set; }
    public float CombatAggressiveness { get; set; }
    public float CombatInitialLevel { get; set; }
    public float CombatInitialGrowth { get; set; }
    public float CombatInitialColonize { get; set; }
    public float CombatMaxDensity { get; set; }
    public float CombatGrowthSpeedFactor { get; set; }
    public float CombatPowerThreatFactor { get; set; }
    public float CombatBattleThreatFactor { get; set; }
    public float CombatBattleExpFactor { get; set; }
}
