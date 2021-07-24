using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Trash;

namespace NebulaNetwork.PacketProcessors.Trash
{
    [RegisterPacketProcessor]
    class TrashSystemResponseDataProcessor : PacketProcessor<TrashSystemResponseDataPacket>
    {
        public override void ProcessPacket(TrashSystemResponseDataPacket packet, NebulaConnection conn)
        {
            using (BinaryUtils.Reader reader = new BinaryUtils.Reader(packet.TrashSystemData))
            {
                GameMain.data.trashSystem.Import(reader.BinaryReader);
            }
        }
    }
}