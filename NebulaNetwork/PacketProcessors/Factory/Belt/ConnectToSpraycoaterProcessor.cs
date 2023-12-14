#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Belt;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Belt;

[RegisterPacketProcessor]
internal class ConnectToSpraycoaterProcessor : PacketProcessor<ConnectToSpraycoaterPacket>
{
    protected override void ProcessPacket(ConnectToSpraycoaterPacket packet, NebulaConnection conn)
    {
        using (Multiplayer.Session.Factories.IsIncomingRequest.On())
        {
            var cargoTraffic = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.cargoTraffic;
            if (cargoTraffic == null)
            {
                return;
            }
            cargoTraffic.ConnectToSpraycoater(packet.SpraycoaterId, packet.CargoBeltId, packet.IncBeltId);
        }
    }
}
