#region

using NebulaAPI.Networking;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld.Planet;

#endregion

namespace NebulaNetwork.PacketProcessors.Players;

[RegisterPacketProcessor]
public class PlayerDashboardProcessor : PacketProcessor<PlayerDashboardPacket>
{
    protected override void ProcessPacket(PlayerDashboardPacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            var player = Players.Get(conn) ?? Players.Get(conn, EConnectionStatus.Syncing);
            if (player?.Data is NebulaModel.DataStructures.PlayerData playerData)
            {
                playerData.DashboardData = packet.DashboardData;
            }
        }
        else
        {
            if (packet.DashboardData != null && packet.DashboardData.Length > 0)
            {
                PlanetManager.PreservedDashboardData = packet.DashboardData;
                if (GameMain.data?.statistics?.charts != null)
                {
                    try
                    {
                        using var reader = new BinaryUtils.Reader(packet.DashboardData);
                        GameMain.data.statistics.charts.Import(reader.BinaryReader);
                        var dashboard = UIRoot.instance?.uiGame?.dashboard;
                        if (dashboard != null)
                        {
                            dashboard.DetermineCharts();
                            dashboard.UpdateCharts();
                        }
                    }
                    catch (System.Exception e)
                    {
                        NebulaModel.Logger.Log.Warn($"Failed to import dashboard data: {e}");
                    }
                }
            }
        }
    }
}
