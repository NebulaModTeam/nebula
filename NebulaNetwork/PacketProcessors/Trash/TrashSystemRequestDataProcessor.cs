using NebulaAPI;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Trash;

namespace NebulaNetwork.PacketProcessors.Trash
{
    [RegisterPacketProcessor]
    class TrashSystemRequestDataProcessor : PacketProcessor<TrashSystemRequestDataPacket>
    {
        public override void ProcessPacket(TrashSystemRequestDataPacket packet, NebulaConnection conn)
        {
            if (IsClient) return;

            using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
            {
                GameMain.data.trashSystem.Export(writer.BinaryWriter);
                conn.SendPacket(new TrashSystemResponseDataPacket(writer.CloseAndGetBytes()));
            }
        }
    }
}