using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Trash;

namespace NebulaNetwork.PacketProcessors.Trash
{
    [RegisterPacketProcessor]
    internal class TrashSystemResponseDataProcessor : PacketProcessor<TrashSystemResponseDataPacket>
    {
        public override void ProcessPacket(TrashSystemResponseDataPacket packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                return;
            }

            using (BinaryUtils.Reader reader = new BinaryUtils.Reader(packet.TrashSystemData))
            {
                GameMain.data.trashSystem.Import(reader.BinaryReader);
            }
            // Wait until WarningDataPacket to assign warningId
            TrashContainer container = GameMain.data.trashSystem.container;
            for (int i = 0; i < container.trashCursor; i++)
            {
                container.trashDataPool[i].warningId = -1;
            }
        }
    }
}