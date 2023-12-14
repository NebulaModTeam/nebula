#region

using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Statistics;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Statistics;

[RegisterPacketProcessor]
internal class StatisticsRequestEventProcessor : PacketProcessor<StatisticsRequestEvent>
{
    private readonly IPlayerManager playerManager;

    public StatisticsRequestEventProcessor()
    {
        playerManager = Multiplayer.Session.Network.PlayerManager;
    }

    public override void ProcessPacket(StatisticsRequestEvent packet, NebulaConnection conn)
    {
        if (IsClient)
        {
            return;
        }

        var player = playerManager.GetPlayer(conn);
        if (player != null)
        {
            if (packet.Event == StatisticEvent.WindowOpened)
            {
                Multiplayer.Session.Statistics.RegisterPlayer(conn, player.Id);

                using (var writer = new BinaryUtils.Writer())
                {
                    Multiplayer.Session.Statistics.ExportAllData(writer.BinaryWriter);
                    conn.SendPacket(new StatisticsDataPacket(writer.CloseAndGetBytes()));
                }
            }
            else if (packet.Event == StatisticEvent.WindowClosed)
            {
                Multiplayer.Session.Statistics.UnRegisterPlayer(player.Id);
            }
        }
    }
}
