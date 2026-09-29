#region

using NebulaAPI.Networking;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld.GameStates;

#endregion

namespace NebulaNetwork.PacketProcessors.Players;

[RegisterPacketProcessor]
public class PlayerReplicatorMultipliersProcessor : PacketProcessor<PlayerReplicatorMultipliersPacket>
{
    protected override void ProcessPacket(PlayerReplicatorMultipliersPacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            var player = Players.Get(conn) ?? Players.Get(conn, EConnectionStatus.Syncing);
            if (player?.Data is NebulaModel.DataStructures.PlayerData playerData)
            {
                playerData.ReplicatorMultipliersData = packet.MultipliersData;
            }
        }
        else
        {
            if (packet.MultipliersData != null && packet.MultipliersData.Length > 0)
            {
                GameStatesManager.ApplyReplicatorMultipliers(packet.MultipliersData);
            }
        }
    }
}
