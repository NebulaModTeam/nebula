using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Trash;
using NebulaWorld.Trash;

namespace NebulaClient.PacketProcessors.Trash
{
    [RegisterPacketProcessor]
    class TrashSystemTrashRemovedProcessor : IPacketProcessor<TrashSystemTrashRemovedPacket>
    {
        public void ProcessPacket(TrashSystemTrashRemovedPacket packet, NebulaConnection conn)
        {
            TrashManager.RemoveTrashFromOtherPlayers = true;
            GameMain.data.trashSystem.container.RemoveTrash(packet.TrashId);
            TrashManager.RemoveTrashFromOtherPlayers = false;
        }
    }
}