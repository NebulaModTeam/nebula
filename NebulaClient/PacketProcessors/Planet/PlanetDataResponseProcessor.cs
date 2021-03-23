using LZ4;
using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Processors;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace NebulaClient.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    public class PlanetDataResponseProcessor : IPacketProcessor<PlanetDataResponse>
    {
        public void ProcessPacket(PlanetDataResponse packet, NebulaConnection conn)
        {
            // We have to track the offset we are currently at in the flattened jagged array as we decode
            int currentOffset = 0;

            for(int i = 0; i < packet.PlanetDataIDs.Length; i++)
            {
                PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetDataIDs[i]);

                Log.Info($"Parsing {packet.PlanetDataBytesLengths[i]} bytes of data for planet {planet.name} (ID: {planet.id})");

                using (MemoryStream ms = new MemoryStream(packet.PlanetDataBytes.Skip(currentOffset).Take(packet.PlanetDataBytesLengths[i]).ToArray()))
                using (LZ4Stream ls = new LZ4Stream(ms, CompressionMode.Decompress))
                using (BufferedStream bs = new BufferedStream(ls, 8192))
                using (BinaryReader br = new BinaryReader(bs))
                {
                    planet.ImportRuntime(br);
                }

                lock (PlanetModelingManager.genPlanetReqList)
                {
                    PlanetModelingManager.genPlanetReqList.Enqueue(planet);
                }

                currentOffset += packet.PlanetDataBytesLengths[i];
            }
        }
    }
}
