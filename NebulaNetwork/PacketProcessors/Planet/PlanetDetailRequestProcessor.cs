using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Planet;
using System.Threading.Tasks;

namespace NebulaNetwork.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    public class PlanetDetailRequestProcessor : PacketProcessor<PlanetDetailRequest>
    {
        public override void ProcessPacket(PlanetDetailRequest packet, NebulaConnection conn)
        {
            if (IsClient)
            {
                return;
            }
            PlanetData planetData = GameMain.galaxy.PlanetById(packet.PlanetID);
            if (!planetData.calculated)
            {
                planetData.calculating = true;
                Task.Run(() =>
                {
                    // Modify from PlanetModelingManager.PlanetCalculateThreadMain()
                    HighStopwatch highStopwatch = new HighStopwatch();
                    highStopwatch.Begin();
                    planetData.data = new PlanetRawData(planetData.precision);
                    planetData.modData = planetData.data.InitModData(planetData.modData);
                    planetData.data.CalcVerts();
                    planetData.aux = new PlanetAuxData(planetData);
                    PlanetAlgorithm planetAlgorithm = PlanetModelingManager.Algorithm(planetData);
                    planetAlgorithm.GenerateTerrain(planetData.mod_x, planetData.mod_y);
                    planetAlgorithm.CalcWaterPercent();
                    if (planetData.type != EPlanetType.Gas)
                    {
                        planetAlgorithm.GenerateVegetables();
                    }
                    if (planetData.type != EPlanetType.Gas)
                    {
                        planetAlgorithm.GenerateVeins();
                    }
                    planetData.CalculateVeinGroups();
                    planetData.GenBirthPoints();
                    planetData.NotifyCalculated();
                    conn.SendPacket(new PlanetDetailResponse(planetData.id, planetData.runtimeVeinGroups, planetData.landPercent));
                    Log.Info($"PlanetCalculateThreadMain: {planetData.displayName} {highStopwatch.duration:F4}s");
                });
                return;
            }
            conn.SendPacket(new PlanetDetailResponse(planetData.id, planetData.runtimeVeinGroups, planetData.landPercent));
        }
    }
}
