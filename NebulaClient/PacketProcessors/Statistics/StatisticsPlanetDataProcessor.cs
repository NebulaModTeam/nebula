using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Logger;
using NebulaModel.Packets.Statistics;
using System.Reflection;

namespace NebulaClient.PacketProcessors.Statistics
{
    [RegisterPacketProcessor]
    class StatisticsPlanetDataProcessor : IPacketProcessor<StatisticsPlanetDataPacket>
    {
        public void ProcessPacket(StatisticsPlanetDataPacket packet, NebulaConnection conn)
        {
            Log.Info($"Parsing Statistics planet data from the server.");
            for(int i = 0; i < packet.PlanetsIds.Length; i++)
            {
                if (GameMain.data.factories[i] == null)
                {
                    GameMain.data.factories[i] = new PlanetFactory();
                    var property = typeof(PlanetFactory).GetProperty("planet", BindingFlags.Public | BindingFlags.Instance);
                    property = property.DeclaringType.GetProperty(property.Name);
                    PlanetData pd = GameMain.galaxy.PlanetById(packet.PlanetsIds[i]);
                    pd.factoryIndex = i;
                    property.SetValue(GameMain.data.factories[i], pd, null);
                }
                if (GameMain.statistics.production.factoryStatPool[i] == null)
                {
                    GameMain.statistics.production.factoryStatPool[i] = new FactoryProductionStat();
                    GameMain.statistics.production.factoryStatPool[i].Init(); 
                }
            }            
        }
    }
}
