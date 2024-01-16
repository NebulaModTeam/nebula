#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Session;

#endregion

namespace NebulaNetwork.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    internal class LobbyUpdateCombatValuesProcessor : PacketProcessor<LobbyUpdateCombatValues>
    {
        protected override void ProcessPacket(LobbyUpdateCombatValues packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                return;
            }

            var gameDesc = UIRoot.instance.galaxySelect.gameDesc;
            if (!gameDesc.isPeaceMode)
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
        }
    }
}
