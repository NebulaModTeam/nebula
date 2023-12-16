namespace NebulaModel.Packets.Session
{
    public class LobbyUpdateCombatValues
    {
        public LobbyUpdateCombatValues() { }

        public LobbyUpdateCombatValues(CombatSettings combatSettings)
        {
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
}
