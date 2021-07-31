using NebulaModel.Attributes;
using Mirror;
using NebulaModel.Packets;
using NebulaModel.Packets.Trash;
using NebulaModel.Networking;

namespace NebulaNetwork.PacketProcessors.Trash
{
    [RegisterPacketProcessor]
    class TrashSystemRequestDataProcessor : PacketProcessor<TrashSystemRequestDataPacket>
    {
        public override void ProcessPacket(TrashSystemRequestDataPacket packet, NetworkConnection conn)
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