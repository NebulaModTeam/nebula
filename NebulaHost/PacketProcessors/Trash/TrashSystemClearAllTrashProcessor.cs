using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Trash;
using NebulaWorld.Trash;

namespace NebulaNetwork.PacketProcessors.Trash
{
    [RegisterPacketProcessor]
    class TrashSystemClearAllTrashProcessor : PacketProcessor<TrashSystemClearAllTrashPacket>
    {
        private PlayerManager playerManager;

        public TrashSystemClearAllTrashProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }
        public override void ProcessPacket(TrashSystemClearAllTrashPacket packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                using (TrashManager.ClearAllTrashFromOtherPlayers.On())
                {
                    GameMain.data.trashSystem.ClearAllTrash();
                }
                playerManager.SendPacketToOtherPlayers(packet, player);
            }
        }
    }
}