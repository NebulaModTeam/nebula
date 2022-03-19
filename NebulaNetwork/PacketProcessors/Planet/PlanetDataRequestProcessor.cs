﻿using BepInEx;
using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Planet;

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
            
            foreach (int planetId in packet.PlanetIDs)
            {
                ThreadingHelper.Instance.StartAsyncInvoke(() =>
                {
                    byte[] data = PlanetCompute(planetId);
                    // use callback to run in mainthread
                    return () => conn.SendPacket(new PlanetDataResponse(planetId, data));
                });
            }
        }

        public static void OnActivePlanetLoaded(PlanetData planet)
        {
            planet.Unload();
            planet.onLoaded -= OnActivePlanetLoaded;
        }

        private static byte[] PlanetCompute(int planetId) 
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

            byte[] data;
            using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
            {
                planet.ExportRuntime(writer.BinaryWriter);
                data = writer.CloseAndGetBytes();
            }
            Log.Info($"Returning terrain for {planet.name} (id:{planet.id} time:{highStopwatch.duration:F4}s)");
            return data;
        }
    }
}
