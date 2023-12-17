#region

using System;
using System.IO;
using NebulaAPI.GameState;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Statistics;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Statistics;

[RegisterPacketProcessor]
internal class StatisticsRequestEventProcessor : PacketProcessor<StatisticsRequestEvent>
{
    private readonly IPlayerManager playerManager = Multiplayer.Session.Network.PlayerManager;

    protected override void ProcessPacket(StatisticsRequestEvent packet, NebulaConnection conn)
    {
        if (IsClient)
        {
            return;
        }

        var player = playerManager.GetPlayer(conn);
        if (player == null)
        {
            return;
        }
        switch (packet.Event)
        {
            case StatisticEvent.WindowOpened:
                {
                    Multiplayer.Session.Statistics.RegisterPlayer(conn, player.Id);

                    using var writer = new BinaryUtils.Writer();
                    Multiplayer.Session.Statistics.ExportAllData(writer.BinaryWriter);
                    conn.SendPacket(new StatisticsDataPacket(writer.CloseAndGetBytes()));
                    break;
                }
            case StatisticEvent.WindowClosed:
                Multiplayer.Session.Statistics.UnRegisterPlayer(player.Id);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(packet), "Unknown event type: " + packet.Event);
        }
    }
}
