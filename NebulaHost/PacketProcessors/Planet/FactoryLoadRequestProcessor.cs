using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Processors;
using NebulaWorld.Statistics;

namespace NebulaHost.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    public class FactoryLoadRequestProcessor : IPacketProcessor<FactoryLoadRequest>
    {
        public void ProcessPacket(FactoryLoadRequest packet, NebulaConnection conn)
        {
            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetID);
            PlanetFactory factory = GameMain.data.GetOrCreateFactory(planet);

            using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
            {
                factory.Export(writer.BinaryWriter);
                conn.SendPacket(new FactoryData(packet.PlanetID, writer.CloseAndGetBytes()));
            }
            conn.SendPacket(StatisticsManager.instance.GetFactoryPlanetIds());
        }
    }
}
