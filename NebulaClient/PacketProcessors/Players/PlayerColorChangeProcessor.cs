using NebulaClient.GameLogic;
using NebulaClient.MonoBehaviours;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Processors;

namespace NebulaClient.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    public class PlayerColorChangeProcessor : IPacketProcessor<PlayerColorChanged>
    {
        private PlayerManager playerManager;

        public PlayerColorChangeProcessor()
        {
            playerManager = MultiplayerClientSession.Instance.PlayerManager;
        }

        public void ProcessPacket(PlayerColorChanged packet, NebulaConnection conn)
        {
            playerManager.GetPlayerById(packet.PlayerId).UpdateColor(packet.Color);
        }
    }
}
