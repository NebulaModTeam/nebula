using NebulaModel;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Trash;
using NebulaWorld;
using NebulaWorld.Trash;

namespace NebulaNetwork.PacketProcessors.Trash
{
    [RegisterPacketProcessor]
    class TrashSystemTrashRemovedProcessor : PacketProcessor<TrashSystemTrashRemovedPacket>
    {
        private IPlayerManager playerManager;

        public TrashSystemTrashRemovedProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(TrashSystemTrashRemovedPacket packet, NebulaConnection conn)
        {
            bool valid = true;

            if (IsHost)
            {
                NebulaPlayer player = playerManager.GetPlayer(conn);
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