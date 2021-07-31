using NebulaModel.Attributes;
using Mirror;
using NebulaModel.Packets;
using NebulaModel.Packets.Trash;
using NebulaModel.Networking;

namespace NebulaNetwork.PacketProcessors.Trash
{
    [RegisterPacketProcessor]
    class TrashSystemResponseDataProcessor : PacketProcessor<TrashSystemResponseDataPacket>
    {
        public override void ProcessPacket(TrashSystemResponseDataPacket packet, NetworkConnection conn)
        {
            if (IsHost) return;

            using (BinaryUtils.Reader reader = new BinaryUtils.Reader(packet.TrashSystemData))
            {
                GameMain.data.trashSystem.Import(reader.BinaryReader);
            }
        }
    }
}