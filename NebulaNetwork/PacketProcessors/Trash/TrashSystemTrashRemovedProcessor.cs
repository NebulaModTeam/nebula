#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Trash;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Trash;

[RegisterPacketProcessor]
internal class TrashSystemTrashRemovedProcessor : PacketProcessor<TrashSystemTrashRemovedPacket>
{
    protected override void ProcessPacket(TrashSystemTrashRemovedPacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            Server.SendPacketExclude(packet, conn);
        }
        using (Multiplayer.Session.Trashes.RemoveTrashFromOtherPlayers.On())
        {
            GameMain.data.trashSystem.RemoveTrash(packet.TrashId);
        }
    }
}
