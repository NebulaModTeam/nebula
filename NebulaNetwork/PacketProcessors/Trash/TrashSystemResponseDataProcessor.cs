#region

using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Trash;

#endregion

namespace NebulaNetwork.PacketProcessors.Trash;

[RegisterPacketProcessor]
internal class TrashSystemResponseDataProcessor : PacketProcessor<TrashSystemResponseDataPacket>
{
    public override void ProcessPacket(TrashSystemResponseDataPacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            return;
        }

        using (var reader = new BinaryUtils.Reader(packet.TrashSystemData))
        {
            GameMain.data.trashSystem.Import(reader.BinaryReader);
        }
        // Wait until WarningDataPacket to assign warningId
        var container = GameMain.data.trashSystem.container;
        for (var i = 0; i < container.trashCursor; i++)
        {
            container.trashDataPool[i].warningId = -1;
        }
    }
}
