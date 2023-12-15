#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Belt;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Belt;

[RegisterPacketProcessor]
internal class ConnectToMonitorProcessor : PacketProcessor<ConnectToMonitorPacket>
{
    protected override void ProcessPacket(ConnectToMonitorPacket packet, NebulaConnection conn)
    {
        using (Multiplayer.Session.Factories.IsIncomingRequest.On())
        {
            var cargoTraffic = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.cargoTraffic;
            if (cargoTraffic == null)
            {
                return;
            }
            cargoTraffic.ConnectToMonitor(packet.MonitorId, packet.BeltId, packet.Offset);
        }
    }
}
