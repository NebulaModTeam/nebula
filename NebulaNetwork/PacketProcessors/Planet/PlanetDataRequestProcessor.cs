using Mirror;
using NebulaModel.Networking;
using System.Collections.Generic;
using Log = NebulaModel.Logger.Log;

namespace NebulaNetwork.PacketProcessors.Planet
{
    public struct PlanetDataRequest : NetworkMessage
    {
        public int[] PlanetIDs;

        public PlanetDataRequest(int[] planetIDs)
        {
            PlanetIDs = planetIDs;
            NebulaModel.Logger.Log.Info($"Creating {GetType()}");
        }

        public static void ProcessPacket(NetworkConnection conn, PlanetDataRequest packet)
        {
            NebulaModel.Logger.Log.Info($"Processing {packet.GetType()}");

            Dictionary<int, byte[]> planetDataToReturn = new Dictionary<int, byte[]>();

            foreach (int planetId in packet.PlanetIDs)
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

                    //Load planet meshes and register callback to unload unneccessary stuff
                    planet.wanted = true;
                    planet.onLoaded += OnActivePlanetLoaded;
                    PlanetModelingManager.modPlanetReqList.Enqueue(planet);

                    if (planet.type != EPlanetType.Gas)
                    {
                        planetAlgorithm.GenerateVegetables();
                        planetAlgorithm.GenerateVeins(false);
                    }
                }

                using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
                {
                    planet.ExportRuntime(writer.BinaryWriter);
                    planetDataToReturn.Add(planetId, writer.CloseAndGetBytes());
                }
            }

            conn.Send(new PlanetDataResponse(planetDataToReturn));
        }

        public static void OnActivePlanetLoaded(PlanetData planet)
        {
            planet.Unload();
            planet.onLoaded -= OnActivePlanetLoaded;
        }
    }
}
