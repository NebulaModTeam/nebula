using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Statistics;
using NebulaWorld.Statistics;

namespace NebulaHost.PacketProcessors.Statistics
{
    [RegisterPacketProcessor]
    class StatisticsRequestEventProcessor : IPacketProcessor<StatisticsRequestEvent>
    {
        private PlayerManager playerManager;

        public StatisticsRequestEventProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public void ProcessPacket(StatisticsRequestEvent packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                if (packet.Event == StatisticEvent.WindowOpened)
                {
                    StatisticsManager.instance.RegisterPlayer(conn, player.Id);

                    using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
                    {
                       StatisticsManager.ExportAllData(writer.BinaryWriter);
                       conn.SendPacket(new StatisticsDataPacket(writer.CloseAndGetBytes()));
                    }
                }
                else if (packet.Event == StatisticEvent.WindowClosed)
                {
                    StatisticsManager.instance.UnRegisterPlayer(player.Id);
                }
            }
        }
    }
}