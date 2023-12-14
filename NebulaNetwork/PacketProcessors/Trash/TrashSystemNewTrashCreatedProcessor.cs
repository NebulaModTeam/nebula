#region

using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Trash;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Trash;

[RegisterPacketProcessor]
internal class TrashSystemNewTrashCreatedProcessor : PacketProcessor<TrashSystemNewTrashCreatedPacket>
{
    private readonly IPlayerManager playerManager;

    public TrashSystemNewTrashCreatedProcessor()
    {
        playerManager = Multiplayer.Session.Network.PlayerManager;
    }

    public override void ProcessPacket(TrashSystemNewTrashCreatedPacket packet, NebulaConnection conn)
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

        if (valid)
        {
            var myId = Multiplayer.Session.World.GenerateTrashOnPlayer(packet);

            //Check if myID is same as the ID from the host
            if (myId != packet.TrashId)
            {
                Multiplayer.Session.Trashes.SwitchTrashWithIds(myId, packet.TrashId);
            }
        }
    }
}
