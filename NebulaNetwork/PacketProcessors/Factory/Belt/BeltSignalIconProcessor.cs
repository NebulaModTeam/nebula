using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Belt;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Factory.Belt
{
    [RegisterPacketProcessor]
    internal class BeltSignalIconProcessor : PacketProcessor<BeltSignalIconPacket>
    {
        public override void ProcessPacket(BeltSignalIconPacket packet, NebulaConnection conn)
        {
            using (Multiplayer.Session.Factories.IsIncomingRequest.On())
            {
                CargoTraffic cargoTraffic = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.cargoTraffic;
                if (cargoTraffic == null)
                {
                    return;
                }
                cargoTraffic.SetBeltSignalIcon(packet.EntityId, packet.SignalId);
            }
        }
    }
}
