using NebulaModel.Attributes;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Statistics;
using NebulaWorld.Statistics;
using System.IO;
using LZ4;
using System.IO.Compression;

namespace NebulaClient.PacketProcessors.Statistics
{
    [RegisterPacketProcessor]
    class StatisticsUpdateProcessor : IPacketProcessor<StatisticUpdateDataPacket>
    {
        public void ProcessPacket(StatisticUpdateDataPacket packet, NebulaConnection conn)
        {
            Log.Info($"Processing Statistics Update Data {packet.StatisticsBinaryData.Length} bytes ");
            StatisticalSnapShot snapshot;
            StatisticsManager.IsIncommingRequest = true;
            using (MemoryStream ms = new MemoryStream(packet.StatisticsBinaryData))
            using (LZ4Stream ls = new LZ4Stream(ms, CompressionMode.Decompress))
            using (BufferedStream bs = new BufferedStream(ls, 8192))
            using (BinaryReader br = new BinaryReader(bs))
            {
                ref FactoryProductionStat[] productionStats = ref GameMain.statistics.production.factoryStatPool;
                int numOfSnapshots = br.ReadInt32();
                for (int i = 0; i < numOfSnapshots; i++)
                {
                    //Clear all current statistical data
                    for (int a = 0; a < GameMain.statistics.production.factoryStatPool.Length; a++)
                    {
                        GameMain.statistics.production.factoryStatPool[a]?.ClearRegisters();
                    }

                    //Load new snapshot
                    snapshot = new StatisticalSnapShot(br);
                    for (int factoryId = 0; factoryId < snapshot.ProductionChangesPerFactory.Length; factoryId++)
                    {
                        if (productionStats[factoryId] == null)
                        {
                            productionStats[factoryId] = new FactoryProductionStat();
                            productionStats[factoryId].Init();
                        }
                        for (int changeId = 0; changeId < snapshot.ProductionChangesPerFactory[factoryId].Count; changeId++)
                        {
                            if (snapshot.ProductionChangesPerFactory[factoryId][changeId].IsProduction)
                            {
                                productionStats[factoryId].productRegister[snapshot.ProductionChangesPerFactory[factoryId][changeId].ProductId] += snapshot.ProductionChangesPerFactory[factoryId][changeId].Amount;
                            }
                            else
                            {
                                productionStats[factoryId].consumeRegister[snapshot.ProductionChangesPerFactory[factoryId][changeId].ProductId] += snapshot.ProductionChangesPerFactory[factoryId][changeId].Amount;
                            }
                        }
                        //Import power system statistics
                        productionStats[factoryId].powerGenRegister = snapshot.PowerGenRegister[factoryId];
                        productionStats[factoryId].powerConRegister = snapshot.PowerConRegister[factoryId];
                        productionStats[factoryId].powerChaRegister = snapshot.PowerChaRegister[factoryId];
                        productionStats[factoryId].powerDisRegister = snapshot.PowerDisRegister[factoryId];

                        //Import fake energy stored values
                        StatisticsManager.FakePowerSystemData = snapshot.EnergyStored;

                        //Import Research statistics
                        productionStats[factoryId].hashRegister = snapshot.HashRegister[factoryId];
                    }
                    GameMain.statistics.production.GameTick(snapshot.CapturedGameTick);
                }
            }
            StatisticsManager.IsIncommingRequest = false;
        }
    }
}
