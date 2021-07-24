using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Trash;
using NebulaWorld.Trash;

namespace NebulaNetwork.PacketProcessors.Trash
{
    [RegisterPacketProcessor]
    class TrashSystemTrashRemovedProcessor : PacketProcessor<TrashSystemTrashRemovedPacket>
    {
        public override void ProcessPacket(TrashSystemTrashRemovedPacket packet, NebulaConnection conn)
        {
            using (TrashManager.RemoveTrashFromOtherPlayers.On())
            {
                GameMain.data.trashSystem.container.RemoveTrash(packet.TrashId);
            }
        }
    }
}