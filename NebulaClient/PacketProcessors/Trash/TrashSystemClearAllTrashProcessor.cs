using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Trash;
using NebulaWorld.Trash;

namespace NebulaClient.PacketProcessors.Trash
{
    [RegisterPacketProcessor]
    class TrashSystemClearAllTrashProcessor : PacketProcessor<TrashSystemClearAllTrashPacket>
    {
        public override void ProcessPacket(TrashSystemClearAllTrashPacket packet, NebulaConnection conn)
        {
            using (TrashManager.ClearAllTrashFromOtherPlayers.On())
            {
                GameMain.data.trashSystem.ClearAllTrash();
            }
        }
    }
}