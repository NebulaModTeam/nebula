using LZ4;
using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Processors;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace NebulaHost.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    public class PlanetDataRequestProcessor : IPacketProcessor<PlanetDataRequest>
    {
        public void ProcessPacket(PlanetDataRequest packet, NebulaConnection conn)
        {
            Dictionary<int, byte[]> planetDataToReturn = new Dictionary<int, byte[]>();

            foreach(int planetId in packet.PlanetIDs)
            {
                PlanetData planet = GameMain.galaxy.PlanetById(planetId);
                Log.Info($"Returning terrain for {planet.name}");

                // NOTE: The following has been picked-n-mixed from "PlanetModelingManager.PlanetComputeThreadMain()"
                // This method is **costly** - do not run it more than is required!
                // It generates the planet on the host and then sends it to the client

                PlanetAlgorithm planetAlgorithm = PlanetModelingManager.Algorithm(planet);

                if (planet.data == null)
                {
                    planet.data = new PlanetRawData(planet.precision);
                    planet.modData = planet.data.InitModData(planet.modData);
                    planet.data.CalcVerts();
                    planet.aux = new PlanetAuxData(planet);
                    planetAlgorithm.GenerateTerrain(planet.mod_x, planet.mod_y);
                    planetAlgorithm.CalcWaterPercent();
                }

                if (planet.factory == null)
                {
                    if (planet.type != EPlanetType.Gas)
                    {
                        planetAlgorithm.GenerateVegetables();
                        planetAlgorithm.GenerateVeins(false);
                    }
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    using (LZ4Stream ls = new LZ4Stream(ms, CompressionMode.Compress))
                    using (BufferedStream bs = new BufferedStream(ls, 8192))
                    using (BinaryWriter bw = new BinaryWriter(bs))
                    {
                        planet.ExportRuntime(bw);
                    }

                    planetDataToReturn.Add(planetId, ms.ToArray());
                }
            }

            conn.SendPacket(new PlanetDataResponse(planetDataToReturn));
        }
    }
}
