using HarmonyLib;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Statistics;

namespace NebulaNetwork.PacketProcessors.Statistics
{
    [RegisterPacketProcessor]
    class StatisticsPlanetDataProcessor : PacketProcessor<StatisticsPlanetDataPacket>
    {
        public override void ProcessPacket(StatisticsPlanetDataPacket packet, NebulaConnection conn)
        {
            var property = AccessTools.DeclaredProperty(typeof(PlanetFactory), "planet");
            for (int i = 0; i < packet.PlanetsIds.Length; i++)
            {
                if (GameMain.data.factories[i] == null)
                {
                    GameMain.data.factories[i] = new PlanetFactory();
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
