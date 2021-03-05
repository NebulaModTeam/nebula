using LiteNetLib;
using NebulaHost.MonoBehaviours;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Processors;
using NebulaWorld;

namespace NebulaHost.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    public class PlayerMovementProcessor : IPacketProcessor<PlayerMovement>
    {
        private PlayerManager playerManager;

        public PlayerMovementProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public void ProcessPacket(PlayerMovement packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            player.Position = packet.Position;
            player.Rotation = packet.Rotation;
            player.BodyRotation = packet.BodyRotation;

            playerManager.SendPacketToOtherPlayers(packet, player, DeliveryMethod.Unreliable);

            SimulatedWorld.UpdateRemotePlayerPosition(packet);
        }
    }
}
