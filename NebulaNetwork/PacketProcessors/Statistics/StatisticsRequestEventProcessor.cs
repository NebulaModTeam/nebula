using NebulaAPI;
using NebulaModel;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Statistics;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Statistics
{
    [RegisterPacketProcessor]
    class StatisticsRequestEventProcessor : PacketProcessor<StatisticsRequestEvent>
    {
        private IPlayerManager playerManager;

        public StatisticsRequestEventProcessor()
        {
            playerManager = ((NetworkProvider)Multiplayer.Session.Network).PlayerManager;
        }

        public override void ProcessPacket(StatisticsRequestEvent packet, NebulaConnection conn)
        {
            if (IsClient) return;

            NebulaPlayer player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                if (packet.Event == StatisticEvent.WindowOpened)
                {
                    Multiplayer.Session.Statistics.RegisterPlayer(conn, player.Id);

                    using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
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
}