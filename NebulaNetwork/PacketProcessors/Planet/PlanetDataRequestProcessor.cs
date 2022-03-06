using BepInEx;
using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Planet;
using System.Collections.Generic;

namespace NebulaNetwork.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    public class PlanetDataRequestProcessor : PacketProcessor<PlanetDataRequest>
    {
        public override void ProcessPacket(PlanetDataRequest packet, NebulaConnection conn)
        {
            if (IsClient)
            {
                return;
            }

            Dictionary<int, byte[]> planetDataToReturn = new Dictionary<int, byte[]>();

            foreach (int planetId in packet.PlanetIDs)
            {
                ThreadingHelper.Instance.StartAsyncInvoke(() =>
                {
                    PlanetCompute(planetId, planetDataToReturn);
                    return () => Callback(conn, planetDataToReturn, packet.PlanetIDs.Length);
                });
            }            
        }

        public static void OnActivePlanetLoaded(PlanetData planet)
        {
            planet.Unload();
            planet.onLoaded -= OnActivePlanetLoaded;
        }

        private static void PlanetCompute(int planetId, Dictionary<int, byte[]> planetDataToReturn) 
        {
            PlanetData planet = GameMain.galaxy.PlanetById(planetId);
            HighStopwatch highStopwatch = new HighStopwatch();
            highStopwatch.Begin();

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
                lock (planetDataToReturn)
                {
                    planetDataToReturn.Add(planetId, writer.CloseAndGetBytes());
                }
            }
            Log.Info($"Returning terrain for {planet.name}, time:{highStopwatch.duration:F5} s");
        }

        private static void Callback(NebulaConnection conn, Dictionary<int, byte[]> planetDataToReturn, int count)
        {
            lock (planetDataToReturn)
            {
                if (planetDataToReturn.Count == count)
                {
                    conn.SendPacket(new PlanetDataResponse(planetDataToReturn));
                    planetDataToReturn.Clear();
                }
            }
        }
    }
}
