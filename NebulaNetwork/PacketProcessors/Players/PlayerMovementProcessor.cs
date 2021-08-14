using NebulaAPI;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    public class PlayerMovementProcessor : PacketProcessor<PlayerMovement>
    {
        private PlayerManager playerManager;

        public PlayerMovementProcessor()
        {
            playerManager = MultiplayerHostSession.Instance?.PlayerManager;
        }

        public override void ProcessPacket(PlayerMovement packet, NebulaConnection conn)
        {
            bool valid = true;
            if (IsHost)
            {
                Player player = playerManager.GetPlayer(conn);
                if (player != null)
                {
                    player.Data.LocalPlanetId = packet.LocalPlanetId;
                    player.Data.UPosition = packet.UPosition;
                    player.Data.Rotation = packet.Rotation;
                    player.Data.BodyRotation = packet.BodyRotation;
                    player.Data.LocalPlanetPosition = packet.LocalPlanetPosition;

                    playerManager.SendPacketToOtherPlayers(packet, player);
                }
                else
                {
                    valid = false;
                }
            }

            if (valid)
            {
                SimulatedWorld.UpdateRemotePlayerPosition(packet);
            }
        }
    }
}
