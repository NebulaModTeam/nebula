using NebulaAPI;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Statistics;
using NebulaWorld.Statistics;

namespace NebulaNetwork.PacketProcessors.Statistics
{
    [RegisterPacketProcessor]
    class StatisticsRequestEventProcessor : PacketProcessor<StatisticsRequestEvent>
    {
        private PlayerManager playerManager;

        public StatisticsRequestEventProcessor()
        {
            playerManager = MultiplayerHostSession.Instance?.PlayerManager;
        }

        public override void ProcessPacket(StatisticsRequestEvent packet, NebulaConnection conn)
        {
            if (IsClient) return;

            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                if (packet.Event == StatisticEvent.WindowOpened)
                {
                    StatisticsManager.Instance.RegisterPlayer(conn, player.Id);

                    using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
                    {
                        StatisticsManager.ExportAllData(writer.BinaryWriter);
                        conn.SendPacket(new StatisticsDataPacket(writer.CloseAndGetBytes()));
                    }
                }
                else if (packet.Event == StatisticEvent.WindowClosed)
                {
                    StatisticsManager.UnRegisterPlayer(player.Id);
                }
            }
        }
    }
}