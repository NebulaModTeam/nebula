#region

using System;
using System.IO;
using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Statistics;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Statistics;

[RegisterPacketProcessor]
internal class StatisticsReferenceSpeedTipProcessor : PacketProcessor<StatisticsReferenceSpeedTipPacket>
{
    protected override void ProcessPacket(StatisticsReferenceSpeedTipPacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            try
            {
                using var writer = new BinaryUtils.Writer();
                Multiplayer.Session.Statistics.GetReferenceSpeedTip(writer.BinaryWriter, packet.ItemId, packet.AstroFilter, packet.ItemCycle, packet.ProductionProtoId);
                packet.BinaryData = writer.CloseAndGetBytes();
                conn.SendPacket(packet);
            }
            catch (Exception ex)
            {
                Log.Warn("StatisticsReferenceSpeedTipPacket request error!");
                Log.Warn(ex);
            }
        }
        if (IsClient)
        {
            try
            {
                using var reader = new BinaryUtils.Reader(packet.BinaryData);
                Multiplayer.Session.Statistics.SetReferenceSpeedTip(reader.BinaryReader, packet.ItemId, packet.AstroFilter, packet.ItemCycle, packet.ProductionProtoId);
            }
            catch (Exception ex)
            {
                Log.Warn("StatisticsReferenceSpeedTipPacket response error!");
                Log.Warn(ex);
            }
        }
    }
}
