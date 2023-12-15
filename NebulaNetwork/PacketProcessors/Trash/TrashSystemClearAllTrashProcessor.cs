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
    private readonly IPlayerManager playerManager;

    public TrashSystemClearAllTrashProcessor()
    {
        playerManager = Multiplayer.Session.Network.PlayerManager;
    }

    protected override void ProcessPacket(TrashSystemClearAllTrashPacket packet, NebulaConnection conn)
    {
        var valid = true;
        if (IsHost)
        {
            var player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                playerManager.SendPacketToOtherPlayers(packet, player);
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
