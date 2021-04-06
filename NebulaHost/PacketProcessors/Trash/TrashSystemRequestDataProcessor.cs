using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Trash;

namespace NebulaHost.PacketProcessors.Trash
{
    [RegisterPacketProcessor]
    class TrashSystemRequestDataProcessor : IPacketProcessor<TrashSystemRequestDataPacket>
    {
        public void ProcessPacket(TrashSystemRequestDataPacket packet, NebulaConnection conn)
        {
            using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
            {
                GameMain.data.trashSystem.Export(writer.BinaryWriter);
                conn.SendPacket(new TrashSystemResponseDataPacket(writer.CloseAndGetBytes()));
            }
        }
    }
}