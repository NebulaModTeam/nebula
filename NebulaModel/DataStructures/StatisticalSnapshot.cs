using System.Collections.Generic;
using System.IO;

namespace NebulaModel.DataStructures
{
    public class StatisticalSnapShot
    {
        //List of statistical changes for each planet that happend in one gameTick
        public List<ProductionChangeStruct>[] ProductionChangesPerFactory;
        public long[] PowerGenerationRegister;
        public long[] PowerConsumptionRegister;
        public long[] PowerChargingRegister;
        public long[] PowerDischargingRegister;
        public long[] HashRegister;
        public long CapturedGameTick;
        public long[] EnergyStored;

        public StatisticalSnapShot(long gameTick, int numOfActiveFactories)
        {
            ProductionChangesPerFactory = new List<ProductionChangeStruct>[numOfActiveFactories];
            for (int i = 0; i < numOfActiveFactories; i++)
            {
                ProductionChangesPerFactory[i] = new List<ProductionChangeStruct>();
            }
            PowerGenerationRegister = new long[numOfActiveFactories];
            PowerConsumptionRegister = new long[numOfActiveFactories];
            PowerChargingRegister = new long[numOfActiveFactories];
            PowerDischargingRegister = new long[numOfActiveFactories];
            HashRegister = new long[numOfActiveFactories];
            EnergyStored = new long[numOfActiveFactories];
            CapturedGameTick = gameTick;
        }

        public StatisticalSnapShot(BinaryReader br)
        {
            CapturedGameTick = br.ReadInt64();
            int factoryCount = br.ReadInt32();

            ProductionChangesPerFactory = new List<ProductionChangeStruct>[factoryCount];
            PowerGenerationRegister = new long[factoryCount];
            PowerConsumptionRegister = new long[factoryCount];
            PowerChargingRegister = new long[factoryCount];
            PowerDischargingRegister = new long[factoryCount];
            EnergyStored = new long[factoryCount];
            HashRegister = new long[factoryCount];

            for (int factoryId = 0; factoryId < factoryCount; factoryId++)
            {
                ProductionChangesPerFactory[factoryId] = new List<ProductionChangeStruct>();
                int changesCount = br.ReadInt32();
                for (int changeId = 0; changeId < changesCount; changeId++)
                {
                    ProductionChangesPerFactory[factoryId].Add(new ProductionChangeStruct(br));
                }
                PowerGenerationRegister[factoryId] = br.ReadInt64();
                PowerConsumptionRegister[factoryId] = br.ReadInt64();
                PowerChargingRegister[factoryId] = br.ReadInt64();
                PowerDischargingRegister[factoryId] = br.ReadInt64();
                EnergyStored[factoryId] = br.ReadInt64();
                HashRegister[factoryId] = br.ReadInt64();
            }
        }

        public void Export(BinaryWriter bw)
        {
            FactoryProductionStat stat;
            bw.Write(CapturedGameTick);

            //Collect production/consumption statistics from factories
            bw.Write(ProductionChangesPerFactory.Length);
            for (int factoryId = 0; factoryId < ProductionChangesPerFactory.Length; factoryId++)
            {
                bw.Write(ProductionChangesPerFactory[factoryId].Count);
                for (int changeId = 0; changeId < ProductionChangesPerFactory[factoryId].Count; changeId++)
                {
                    ProductionChangesPerFactory[factoryId][changeId].Export(bw);
                }
                stat = GameMain.statistics.production.factoryStatPool[factoryId];

                //Collect info about power system of the factory
                bw.Write(PowerGenerationRegister[factoryId]);
                bw.Write(PowerConsumptionRegister[factoryId]);
                bw.Write(PowerChargingRegister[factoryId]);
                bw.Write(PowerDischargingRegister[factoryId]);
                bw.Write(EnergyStored[factoryId]);
                bw.Write(HashRegister[factoryId]);
            }
        }
    }
}
