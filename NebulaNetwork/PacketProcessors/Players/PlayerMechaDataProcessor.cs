#region

using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Players;

[RegisterPacketProcessor]
internal class PlayerMechaDataProcessor : PacketProcessor<PlayerMechaData>
{
    private readonly IPlayerManager playerManager;

    public PlayerMechaDataProcessor()
    {
        playerManager = Multiplayer.Session.Network.PlayerManager;
    }

    public override void ProcessPacket(PlayerMechaData packet, NebulaConnection conn)
    {
        if (IsClient)
        {
            return;
        }

        playerManager.UpdateMechaData(packet.Data, conn);
    }
}
