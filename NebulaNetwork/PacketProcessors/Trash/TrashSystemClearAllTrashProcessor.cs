using NebulaAPI;
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
            playerManager = MultiplayerHostSession.Instance?.PlayerManager;
        }

        public override void ProcessPacket(TrashSystemClearAllTrashPacket packet, NebulaConnection conn)
        {
            bool valid = true;
            if (IsHost)
            {
                Player player = playerManager.GetPlayer(conn);
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
                using (TrashManager.ClearAllTrashFromOtherPlayers.On())
                {
                    GameMain.data.trashSystem.ClearAllTrash();
                }
            }
        }
    }
}