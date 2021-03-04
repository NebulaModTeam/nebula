using NebulaClient.GameLogic;
using NebulaClient.MonoBehaviours;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Processors;

namespace NebulaClient.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    public class PlayerMovementProcessor : IPacketProcessor<PlayerMovement>
    {
        private PlayerManager playerManager;
        public PlayerMovementProcessor()
        {
            playerManager = MultiplayerClientSession.Instance.PlayerManager;
        }

        public void ProcessPacket(PlayerMovement packet, NebulaConnection conn)
        {
            playerManager.GetPlayerModelById(packet.PlayerId)?.Movement.UpdatePosition(packet);
        }
    }
}
