using NebulaAPI;
using NebulaModel;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    public class PlayerAnimationUpdateProcessor : PacketProcessor<PlayerAnimationUpdate>
    {
        private IPlayerManager playerManager;

        public PlayerAnimationUpdateProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(PlayerAnimationUpdate packet, NebulaConnection conn)
        {
            bool valid = true;

            if (IsHost)
            {
                NebulaPlayer player = playerManager.GetPlayer(conn);
                if (player != null)
                {
                    packet.PlayerId = player.Id;
                    playerManager.SendPacketToOtherPlayers(packet, player);
                }
                else
                {
                    valid = false;
                }
            }

            if (valid)
            {
                Multiplayer.Session.World.UpdateRemotePlayerAnimation(packet);
            }
        }
    }
}
