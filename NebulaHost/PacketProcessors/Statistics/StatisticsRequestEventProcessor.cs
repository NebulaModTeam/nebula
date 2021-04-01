using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Statistics;
using System.IO;
using System.IO.Compression;
using LZ4;

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

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (LZ4Stream ls = new LZ4Stream(ms, CompressionMode.Compress))
                        using (BufferedStream bs = new BufferedStream(ls, 8192))
                        using (BinaryWriter bw = new BinaryWriter(bs))
                        {
                            StatisticsManager.ExportAllData(bw);
                        }
                        conn.SendPacket(new StatisticsDataPacket(ms.ToArray()));
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