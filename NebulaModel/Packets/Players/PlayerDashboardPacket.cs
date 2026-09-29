namespace NebulaModel.Packets.Players;

public class PlayerDashboardPacket
{
    public PlayerDashboardPacket() { }

    public PlayerDashboardPacket(ushort playerId, byte[] dashboardData)
    {
        PlayerId = playerId;
        DashboardData = dashboardData;
    }

    public ushort PlayerId { get; set; }
    public byte[] DashboardData { get; set; }
}
