#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Session;

#endregion

namespace NebulaNetwork.PacketProcessors.Session;

[RegisterPacketProcessor]
internal class LobbyUpdateValuesProcessor : PacketProcessor<LobbyUpdateValues>
{
    protected override void ProcessPacket(LobbyUpdateValues packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            return;
        }

        var gameDesc = new GameDesc();
        gameDesc.SetForNewGame(packet.GalaxyAlgo, packet.GalaxySeed, packet.StarCount, 1, packet.ResourceMultiplier);
        gameDesc.isSandboxMode = packet.IsSandboxMode;
        gameDesc.isPeaceMode = packet.IsPeaceMode;
        if (!packet.IsPeaceMode)
        {
            gameDesc.combatSettings.aggressiveness = packet.CombatAggressiveness;
            gameDesc.combatSettings.initialLevel = packet.CombatInitialLevel;
            gameDesc.combatSettings.initialGrowth = packet.CombatInitialGrowth;
            gameDesc.combatSettings.initialColonize = packet.CombatInitialColonize;
            gameDesc.combatSettings.maxDensity = packet.CombatMaxDensity;
            gameDesc.combatSettings.growthSpeedFactor = packet.CombatGrowthSpeedFactor;
            gameDesc.combatSettings.powerThreatFactor = packet.CombatPowerThreatFactor;
            gameDesc.combatSettings.battleThreatFactor = packet.CombatBattleThreatFactor;
            gameDesc.combatSettings.battleExpFactor = packet.CombatBattleExpFactor;
        }

        UIRoot.instance.galaxySelect.gameDesc = gameDesc;
        UIRoot.instance.galaxySelect.SetStarmapGalaxy();
        UIRoot.instance.galaxySelect.sandboxToggle.isOn = packet.IsSandboxMode;
    }
}
