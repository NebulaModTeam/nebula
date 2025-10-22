#region

using System.IO;
using BepInEx;
using BepInEx.Configuration;

#endregion

namespace NebulaModel;

public static class GameDescSettings
{
    private const string GAMEDESC_SAVE_FILE = "nebulaGameDescSettings.cfg";

    public static GameDesc SetFromConfigFile(GameDesc gameDesc)
    {
        var customFile = new ConfigFile(Path.Combine(Paths.ConfigPath, GAMEDESC_SAVE_FILE), true);

        var galaxySeed = customFile.Bind("Basic", "galaxySeed", -1,
            "Cluster Seed. Negative value: Random or remain the same.").Value;
        if (galaxySeed >= 0)
        {
            gameDesc.galaxySeed = galaxySeed;
        }

        var starCount = customFile.Bind("Basic", "starCount", -1,
            "Number of Stars. Negative value: Default(64) or remain the same.").Value;
        if (starCount >= 0)
        {
            gameDesc.starCount = starCount;
        }

        var resourceMultiplier = customFile.Bind("Basic", "resourceMultiplier", -1f,
            "Resource Multiplier. Infinite = 100. Negative value: Default(1.0f) or remain the same.").Value;
        if (resourceMultiplier >= 0f)
        {
            gameDesc.resourceMultiplier = resourceMultiplier;
        }

        gameDesc.isPeaceMode = customFile.Bind("General", "isPeaceMode", false,
            "False: Enable enemy force (combat mode)").Value;
        gameDesc.isSandboxMode = customFile.Bind("General", "isSandboxMode", false,
            "True: Enable creative mode").Value;

        gameDesc.combatSettings.aggressiveness = customFile.Bind("Combat", "aggressiveness", 1f,
            new ConfigDescription("Aggressiveness (Dummy = -1, Rampage = 3)", new AcceptableValueList<float>(-1f, 0f, 0.5f, 1f, 2f, 3f))).Value;
        gameDesc.combatSettings.initialLevel = customFile.Bind("Combat", "initialLevel", 0,
            new ConfigDescription("Initial Level (Original range: 0 to 10)", new AcceptableValueRange<int>(0, 30))).Value;
        gameDesc.combatSettings.initialGrowth = customFile.Bind("Combat", "initialGrowth", 1f,
            "Initial Growth (Original range: 0 to 200%)").Value;
        gameDesc.combatSettings.initialColonize = customFile.Bind("Combat", "initialColonize", 1f,
            "Initial Occupation (Original range: 1% to 200%").Value;
        gameDesc.combatSettings.maxDensity = customFile.Bind("Combat", "maxDensity", 1f,
            "Max Density (Original range: 1 to 3)").Value;
        gameDesc.combatSettings.growthSpeedFactor = customFile.Bind("Combat", "growthSpeedFactor", 1f,
            "Growth Speed (Original range: 25% to 300%)").Value;
        gameDesc.combatSettings.powerThreatFactor = customFile.Bind("Combat", "powerThreatFactor", 1f,
            "Power Threat Factor (Original range: 1% to 1000%)").Value;
        gameDesc.combatSettings.battleThreatFactor = customFile.Bind("Combat", "battleThreatFactor", 1f,
            "Combat Threat Factor (Original range: 1% to 1000%)").Value;
        gameDesc.combatSettings.battleExpFactor = customFile.Bind("Combat", "battleExpFactor", 1f,
            "Combat XP Factor (Original range: 1% to 1000%)").Value;

        return gameDesc;
    }
}
