using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Planet;
using NebulaWorld.Statistics;

namespace NebulaNetwork.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    public class FactoryLoadRequestProcessor : PacketProcessor<FactoryLoadRequest>
    {
        public override void ProcessPacket(FactoryLoadRequest packet, NebulaConnection conn)
        {
            if (IsClient) return;

            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetID);
            PlanetFactory factory = GameMain.data.GetOrCreateFactory(planet);

            using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
            {
                factory.Export(writer.BinaryWriter);
                foreach (var serializer in NebulaModAPI.FactorySerializers)
                {
                    serializer.Export(factory, writer.BinaryWriter);
                }
                conn.SendPacket(new FactoryData(packet.PlanetID, writer.CloseAndGetBytes()));
            }
            conn.SendPacket(StatisticsManager.GetFactoryPlanetIds());
        }
    }
}
