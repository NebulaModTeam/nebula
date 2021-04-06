using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Trash;
using NebulaWorld.Trash;

namespace NebulaHost.PacketProcessors.Trash
{
    [RegisterPacketProcessor]
    class TrashSystemTrashRemovedProcessor : IPacketProcessor<TrashSystemTrashRemovedPacket>
    {
        private PlayerManager playerManager;

        public TrashSystemTrashRemovedProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public void ProcessPacket(TrashSystemTrashRemovedPacket packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                TrashManager.RemoveTrashFromOtherPlayers = true;
                GameMain.data.trashSystem.container.RemoveTrash(packet.TrashId);
                TrashManager.RemoveTrashFromOtherPlayers = false;

                //Send to other players trash with valid ID
                playerManager.SendPacketToOtherPlayers(packet, player);
            }
        }
    }
}