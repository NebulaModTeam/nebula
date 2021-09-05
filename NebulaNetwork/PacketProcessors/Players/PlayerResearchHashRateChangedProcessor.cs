using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    public class PlayerResearchHashRateChangedProcessor : PacketProcessor<PlayerResearchHashRateChanged>
    {
        public PlayerResearchHashRateChangedProcessor() { }

        public override void ProcessPacket(PlayerResearchHashRateChanged packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                Multiplayer.Session.Network.PlayerManager.GetPlayer(conn).Data.Mecha.ResearchHashRate = packet.ResearchHashRate;
            }
        }
    }
}
