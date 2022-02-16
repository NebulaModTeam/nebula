using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Belt;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Factory.Belt
{
    [RegisterPacketProcessor]
    internal class BeltSignalNumberProcessor : PacketProcessor<BeltSignalNumberPacket>
    {
        public override void ProcessPacket(BeltSignalNumberPacket packet, NebulaConnection conn)
        {
            using (Multiplayer.Session.Factories.IsIncomingRequest.On())
            {
                CargoTraffic cargoTraffic = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.cargoTraffic;
                if (cargoTraffic == null)
                {
                    return;
                }
                cargoTraffic.SetBeltSignalNumber(packet.EntityId, packet.Number);
            }
        }
    }
}
