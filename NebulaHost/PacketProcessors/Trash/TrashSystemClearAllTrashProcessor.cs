using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Trash;
using NebulaWorld.Trash;

namespace NebulaHost.PacketProcessors.Trash
{
    [RegisterPacketProcessor]
    class TrashSystemClearAllTrashProcessor : IPacketProcessor<TrashSystemClearAllTrashPacket>
    {
        private PlayerManager playerManager;

        public TrashSystemClearAllTrashProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }
        public void ProcessPacket(TrashSystemClearAllTrashPacket packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                TrashManager.ClearAllTrashFromOtherPlayers = true;
                GameMain.data.trashSystem.ClearAllTrash();
                TrashManager.ClearAllTrashFromOtherPlayers = false;
                playerManager.SendPacketToOtherPlayers(packet, player);
            }
        }
    }
}