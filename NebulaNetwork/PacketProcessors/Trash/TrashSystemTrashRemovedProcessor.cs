#region

using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Trash;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Trash;

[RegisterPacketProcessor]
internal class TrashSystemTrashRemovedProcessor : PacketProcessor<TrashSystemTrashRemovedPacket>
{
    public override void ProcessPacket(TrashSystemTrashRemovedPacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            Multiplayer.Session.Network.PlayerManager.SendPacketToOtherPlayers(packet, conn);
        }
        using (Multiplayer.Session.Trashes.RemoveTrashFromOtherPlayers.On())
        {
            GameMain.data.trashSystem.RemoveTrash(packet.TrashId);
        }
    }
}
