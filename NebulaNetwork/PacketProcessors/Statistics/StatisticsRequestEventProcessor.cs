using NebulaModel.Attributes;
using Mirror;
using NebulaModel.Packets;
using NebulaModel.Packets.Statistics;
using NebulaWorld.Statistics;
using NebulaModel.Networking;

namespace NebulaNetwork.PacketProcessors.Statistics
{
    [RegisterPacketProcessor]
    class StatisticsRequestEventProcessor : PacketProcessor<StatisticsRequestEvent>
    {
        private PlayerManager playerManager;

        public StatisticsRequestEventProcessor()
        {
            playerManager = MultiplayerHostSession.Instance != null ? MultiplayerHostSession.Instance.PlayerManager : null;
        }

        public override void ProcessPacket(StatisticsRequestEvent packet, NetworkConnection conn)
        {
            if (IsClient) return;

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