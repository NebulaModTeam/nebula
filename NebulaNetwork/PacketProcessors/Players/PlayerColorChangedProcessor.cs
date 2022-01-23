using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    public class PlayerColorChangedProcessor : PacketProcessor<PlayerAppearanceChanged>
    {
        private readonly IPlayerManager playerManager;

        public PlayerColorChangedProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(PlayerAppearanceChanged packet, NebulaConnection conn)
        {
            bool valid = true;

            if (IsHost)
            {
                INebulaPlayer player = playerManager.GetPlayer(conn);
                if (player != null)
                {
                    player.Data.MechaAppearance = packet.Appearance;
                    playerManager.SendPacketToOtherPlayers(packet, player);
                }
                else
                {
                    valid = false;
                }
            }

            if (valid)
            {
                Multiplayer.Session.World.UpdatePlayerAppearance(packet.PlayerId, packet.Appearance);
            }
        }
    }
}
