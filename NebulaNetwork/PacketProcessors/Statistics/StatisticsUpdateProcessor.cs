#region

using NebulaAPI.Packets;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Statistics;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Statistics;

[RegisterPacketProcessor]
internal class StatisticsUpdateProcessor : PacketProcessor<StatisticUpdateDataPacket>
{
    protected override void ProcessPacket(StatisticUpdateDataPacket packet, NebulaConnection conn)
    {
        using (Multiplayer.Session.Statistics.IsIncomingRequest.On())
        {
            using var reader = new BinaryUtils.Reader(packet.StatisticsBinaryData);
            var itemChanged = false;
            ref var productionStats = ref GameMain.statistics.production.factoryStatPool;
            var numOfSnapshots = reader.BinaryReader.ReadInt32();
            for (var i = 0; i < numOfSnapshots; i++)
            {
                //Load new snapshot
                var snapshot = new StatisticalSnapShot(reader.BinaryReader);
                for (var factoryId = 0; factoryId < snapshot.ProductionChangesPerFactory.Length; factoryId++)
                {
                    if (productionStats[factoryId] == null)
                    {
                        productionStats[factoryId] = new FactoryProductionStat();
                        productionStats[factoryId].Init();
                    }
                    //Clear current statistical data
                    productionStats[factoryId].PrepareTick();

                    for (var changeId = 0; changeId < snapshot.ProductionChangesPerFactory[factoryId].Count; changeId++)
                    {
                        var productionChange = snapshot.ProductionChangesPerFactory[factoryId][changeId];
                        if (productionChange.IsProduction)
                        {
                            productionStats[factoryId].productRegister[productionChange.ProductId] +=
                                productionChange.Amount;
                        }
                        else
                        {
                            productionStats[factoryId].consumeRegister[productionChange.ProductId] +=
                                productionChange.Amount;
                        }
                    }
                    //Import power system statistics
                    productionStats[factoryId].powerGenRegister = snapshot.PowerGenerationRegister[factoryId];
                    productionStats[factoryId].powerConRegister = snapshot.PowerConsumptionRegister[factoryId];
                    productionStats[factoryId].powerChaRegister = snapshot.PowerChargingRegister[factoryId];
                    productionStats[factoryId].powerDisRegister = snapshot.PowerDischargingRegister[factoryId];

                    //Import fake energy stored values
                    Multiplayer.Session.Statistics.PowerEnergyStoredData = snapshot.EnergyStored;

                    //Import Research statistics
                    productionStats[factoryId].hashRegister = snapshot.HashRegister[factoryId];

                    //Process changed registers. FactoryProductionStat.AfterTick() is empty currently so we ignore it.
                    productionStats[factoryId].GameTick(snapshot.CapturedGameTick);
                    itemChanged |= productionStats[factoryId].itemChanged;
                }
            }
            //Trigger GameMain.statistics.production.onItemChange() event when itemChanged is true
            if (!itemChanged)
            {
                return;
            }
            UIRoot.instance.uiGame.statWindow.OnItemChange();
        }
    }
}
