#region

using NebulaAPI.GameState;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Trash;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Trash;

[RegisterPacketProcessor]
internal class TrashSystemClearAllTrashProcessor : PacketProcessor<TrashSystemClearAllTrashPacket>
{
    protected override void ProcessPacket(TrashSystemClearAllTrashPacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            Server.SendPacketExclude(packet, conn);
        }
        using (Multiplayer.Session.Trashes.IsIncomingRequest.On())
        {
            GameMain.data.trashSystem.ClearAllTrash();
        }
    }
}
