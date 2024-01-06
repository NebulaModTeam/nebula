#region

using NebulaAPI.Extensions;
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

    public TrashSystemClearAllTrashProcessor()
    {
    }

    protected override void ProcessPacket(TrashSystemClearAllTrashPacket packet, NebulaConnection conn)
    {
        var valid = true;
        if (IsHost)
        {
            var player = Players.Connected().GetPlayer(conn);
            if (player != null)
            {
                Server.SendToAllExcept(packet, conn);
            }
            else
            {
                valid = false;
            }
        }

        if (!valid)
        {
            return;
        }
        using (Multiplayer.Session.Trashes.ClearAllTrashFromOtherPlayers.On())
        {
            GameMain.data.trashSystem.ClearAllTrash();
        }
    }
}
