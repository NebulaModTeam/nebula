using LiteNetLib;
using NebulaModel.Attributes;
using NebulaModel.Logger;
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
            player.Data.Position = packet.Position;
            player.Data.Rotation = packet.Rotation;
            player.Data.BodyRotation = packet.BodyRotation;

            playerManager.SendPacketToOtherPlayers(packet, player, DeliveryMethod.Unreliable);

            SimulatedWorld.UpdateRemotePlayerPosition(packet);
        }
    }
}
