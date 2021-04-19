using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Trash;
using NebulaWorld.Trash;

namespace NebulaClient.PacketProcessors.Trash
{
    [RegisterPacketProcessor]
    class TrashSystemClearAllTrashProcessor : IPacketProcessor<TrashSystemClearAllTrashPacket>
    {
        public void ProcessPacket(TrashSystemClearAllTrashPacket packet, NebulaConnection conn)
        {
            using (TrashManager.ClearAllTrashFromOtherPlayers.On())
            {
                GameMain.data.trashSystem.ClearAllTrash();
            }
        }
    }
}