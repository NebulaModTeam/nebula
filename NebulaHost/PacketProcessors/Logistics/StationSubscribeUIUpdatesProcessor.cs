using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using NebulaWorld.Logistics;

namespace NebulaHost.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class StationSubscribeUIUpdatesProcessor: IPacketProcessor<StationSubscribeUIUpdates>
    {
        public void ProcessPacket(StationSubscribeUIUpdates packet, NebulaConnection conn)
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
