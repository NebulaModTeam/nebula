using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;

namespace NebulaHost.PacketProcessors.Factory
{
    [RegisterPacketProcessor]
    public class DestructEntityRequestProcessor : IPacketProcessor<DestructEntityRequest>
    {
        public void ProcessPacket(DestructEntityRequest packet, NebulaConnection conn)
        {
            int protoId = 0;
            FactoryManager.IsIncommingRequest = true;
            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
            planet.factory.DestructFinally(GameMain.mainPlayer, packet.ObjId, ref protoId);
            FactoryManager.IsIncommingRequest = false;
        }
    }
}
