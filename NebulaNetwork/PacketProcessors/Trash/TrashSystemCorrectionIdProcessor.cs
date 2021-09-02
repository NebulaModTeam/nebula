using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Trash;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Trash
{
    [RegisterPacketProcessor]
    class TrashSystemCorrectionIdProcessor : PacketProcessor<TrashSystemCorrectionIdPacket>
    {
        public override void ProcessPacket(TrashSystemCorrectionIdPacket packet, NebulaConnection conn)
        {
            if (packet.OriginalId != packet.NewId)
            {
                //Server sent correction packet for the trashId
                //Switch item on position NewId, with item on position OriginalId
                Multiplayer.Session.Trashes.SwitchTrashWithIds(packet.OriginalId, packet.NewId);
            }
        }
    }
}