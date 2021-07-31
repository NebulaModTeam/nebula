using NebulaModel.Attributes;
using NebulaModel.DataStructures;
using Mirror;
using NebulaModel.Packets;
using NebulaModel.Packets.Statistics;
using NebulaWorld.Statistics;
using NebulaModel.Networking;

namespace NebulaNetwork.PacketProcessors.Statistics
{
    [RegisterPacketProcessor]
    class StatisticsUpdateProcessor : PacketProcessor<StatisticUpdateDataPacket>
    {
        public override void ProcessPacket(StatisticUpdateDataPacket packet, NetworkConnection conn)
        {
            StatisticalSnapShot snapshot;
            using (StatisticsManager.IsIncomingRequest.On())
            {
                using (BinaryUtils.Reader reader = new BinaryUtils.Reader(packet.StatisticsBinaryData))
                {
                    ref FactoryProductionStat[] productionStats = ref GameMain.statistics.production.factoryStatPool;
                    int numOfSnapshots = reader.BinaryReader.ReadInt32();
                    for (int i = 0; i < numOfSnapshots; i++)
                    {
                        //Clear all current statistical data
                        for (int a = 0; a < GameMain.statistics.production.factoryStatPool.Length; a++)
                        {
                            GameMain.statistics.production.factoryStatPool[a]?.ClearRegisters();
                        }

                        //Load new snapshot
                        snapshot = new StatisticalSnapShot(reader.BinaryReader);
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
                            productionStats[factoryId].powerGenRegister = snapshot.PowerGenerationRegister[factoryId];
                            productionStats[factoryId].powerConRegister = snapshot.PowerConsumptionRegister[factoryId];
                            productionStats[factoryId].powerChaRegister = snapshot.PowerChargingRegister[factoryId];
                            productionStats[factoryId].powerDisRegister = snapshot.PowerDischargingRegister[factoryId];

                            //Import fake energy stored values
                            StatisticsManager.PowerEnergyStoredData = snapshot.EnergyStored;

                            //Import Research statistics
                            productionStats[factoryId].hashRegister = snapshot.HashRegister[factoryId];
                        }
                        GameMain.statistics.production.GameTick(snapshot.CapturedGameTick);
                    }
                }
            }
        }
    }
}
