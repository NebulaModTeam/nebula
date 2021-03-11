using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Processors;
using NebulaWorld;

namespace NebulaClient.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    class localPlanetSyncProcessor : IPacketProcessor<localPlanetSyncPckt>
    {
        public void ProcessPacket(localPlanetSyncPckt packet, NebulaConnection conn)
        {
            SimulatedWorld.UpdateRemotePlayerLocalPlanetId(packet);
        }
    }
}
