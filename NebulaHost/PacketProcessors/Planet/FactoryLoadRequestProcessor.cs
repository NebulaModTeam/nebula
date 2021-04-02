using LZ4;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Processors;
using NebulaWorld.Statistics;
using System.IO;
using System.IO.Compression;

namespace NebulaHost.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    public class FactoryLoadRequestProcessor : IPacketProcessor<FactoryLoadRequest>
    {
        public void ProcessPacket(FactoryLoadRequest packet, NebulaConnection conn)
        {
            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetID);
            PlanetFactory factory = GameMain.data.GetOrCreateFactory(planet);

            using (MemoryStream ms = new MemoryStream())
            {
                using (LZ4Stream ls = new LZ4Stream(ms, CompressionMode.Compress))
                using (BufferedStream bs = new BufferedStream(ls, 8192))
                using (BinaryWriter bw = new BinaryWriter(bs))
                {//Send update for the factory count and their planetsIds for statistics
                    bw.Write(GameMain.data.factoryCount);
                    bw.Write(planet.factoryIndex);
                    factory.Export(bw);
                }

                conn.SendPacket(new FactoryData(packet.PlanetID, ms.ToArray()));
            }
            conn.SendPacket(StatisticsManager.instance.GetFactoryPlanetIds());
        }
    }
}
