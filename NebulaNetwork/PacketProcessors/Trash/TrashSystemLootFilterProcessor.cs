#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Trash;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Trash;

[RegisterPacketProcessor]
internal class TrashSystemLootFilterProcessor : PacketProcessor<TrashSystemLootFilterPacket>
{
    protected override void ProcessPacket(TrashSystemLootFilterPacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            Multiplayer.Session.Network.SendPacketExclude(packet, conn);
        }

        using var reader = new BinaryUtils.Reader(packet.EnemyDropBansData);
        GameMain.data.trashSystem.enemyDropBans.Clear();
        var count = reader.BinaryReader.ReadInt32();
        for (var i = 0; i < count; i++)
        {
            GameMain.data.trashSystem.enemyDropBans.Add(reader.BinaryReader.ReadInt32());
        }
    }
}
