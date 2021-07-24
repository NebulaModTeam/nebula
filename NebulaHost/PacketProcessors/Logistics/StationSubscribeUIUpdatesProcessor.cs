using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets;
using NebulaWorld.Logistics;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class StationSubscribeUIUpdatesProcessor: PacketProcessor<StationSubscribeUIUpdates>
    {
        public override void ProcessPacket(StationSubscribeUIUpdates packet, NebulaConnection conn)
        {
            if (packet.Subscribe)
            {
                StationUIManager.AddSubscriber(packet.PlanetId, packet.StationId, packet.StationGId, conn);
            }
            else
            {
                StationUIManager.RemoveSubscriber(packet.PlanetId, packet.StationId, packet.StationGId, conn);
            }
        }
    }
}
