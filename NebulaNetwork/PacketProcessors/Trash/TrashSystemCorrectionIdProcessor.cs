using NebulaModel.Attributes;
using Mirror;
using NebulaModel.Packets;
using NebulaModel.Packets.Trash;
using NebulaWorld.Trash;

namespace NebulaNetwork.PacketProcessors.Trash
{
    [RegisterPacketProcessor]
    class TrashSystemCorrectionIdProcessor : PacketProcessor<TrashSystemCorrectionIdPacket>
    {
        public override void ProcessPacket(TrashSystemCorrectionIdPacket packet, NetworkConnection conn)
        {
            if (packet.OriginalId != packet.NewId)
            {
                //Server sent correction packet for the trashId
                //Switch item on position NewId, with item on position OriginalId
                TrashManager.SwitchTrashWithIds(packet.OriginalId, packet.NewId);
            }
        }
    }
}