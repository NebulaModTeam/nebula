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
            if (packet.subscribe)
            {
                StationUIManager.AddSubscriber(packet.stationGId, conn);
                // TODO: sync current state to client
            }
            else
            {
                StationUIManager.RemoveSubscriber(packet.stationGId, conn);
            }
        }
    }
}
