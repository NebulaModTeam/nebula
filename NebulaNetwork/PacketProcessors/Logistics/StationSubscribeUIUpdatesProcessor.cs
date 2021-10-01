using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    internal class StationSubscribeUIUpdatesProcessor : PacketProcessor<StationSubscribeUIUpdates>
    {
        public override void ProcessPacket(StationSubscribeUIUpdates packet, NebulaConnection conn)
        {
            if (IsClient)
            {
                return;
            }

            if (packet.Subscribe)
            {
                Multiplayer.Session.StationsUI.AddSubscriber(packet.PlanetId, packet.StationId, packet.StationGId, conn);
            }
            else
            {
                Multiplayer.Session.StationsUI.RemoveSubscriber(packet.PlanetId, packet.StationId, packet.StationGId, conn);
            }
        }
    }
}
