using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Trash;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Trash
{
    [RegisterPacketProcessor]
    internal class TrashSystemTrashRemovedProcessor : PacketProcessor<TrashSystemTrashRemovedPacket>
    {
        private readonly IPlayerManager playerManager;

        public TrashSystemTrashRemovedProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(TrashSystemTrashRemovedPacket packet, NebulaConnection conn)
        {
            bool valid = true;

            if (IsHost)
            {
                INebulaPlayer player = playerManager.GetPlayer(conn);
                if (player != null)
                {
                    playerManager.SendPacketToOtherPlayers(packet, player);
                }
                else
                {
                    valid = false;
                }
            }

            if (valid)
            {
                using (Multiplayer.Session.Trashes.RemoveTrashFromOtherPlayers.On())
                {
                    GameMain.data.trashSystem.container.RemoveTrash(packet.TrashId);
                }
            }
        }
    }
}