using System.Collections.Generic;
using System.IO;

namespace NebulaModel.DataStructures
{
    public class StatisticalSnapShot
    {
        //List of statistical changes for each planet that happend in one gameTick
        public List<ProductionChangeStruct>[] ProductionChangesPerFactory;
        public long[] PowerGenRegister;
        public long[] PowerConRegister;
        public long[] PowerChaRegister;
        public long[] PowerDisRegister;
        public long[] HashRegister;
        public long CapturedGameTick;
        public long[] EnergyStored;

        public StatisticalSnapShot(long gameTick, int numOfActiveFactories)
        {
            ProductionChangesPerFactory = new List<ProductionChangeStruct>[numOfActiveFactories];
            for(int i = 0; i < numOfActiveFactories; i++)
            {
                ProductionChangesPerFactory[i] = new List<ProductionChangeStruct>();
            }
            PowerGenRegister = new long[numOfActiveFactories];
            PowerConRegister = new long[numOfActiveFactories];
            PowerChaRegister = new long[numOfActiveFactories];
            PowerDisRegister = new long[numOfActiveFactories];
            HashRegister = new long[numOfActiveFactories];
            EnergyStored = new long[numOfActiveFactories];
            CapturedGameTick = gameTick;
        }

        public StatisticalSnapShot(BinaryReader br)
        {
            CapturedGameTick = br.ReadInt64();
            int factoryCount = br.ReadInt32();

            ProductionChangesPerFactory = new List<ProductionChangeStruct>[factoryCount];
            PowerGenRegister = new long[factoryCount];
            PowerConRegister = new long[factoryCount];
            PowerChaRegister = new long[factoryCount];
            PowerDisRegister = new long[factoryCount];
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
                PowerGenRegister[factoryId] = br.ReadInt64();
                PowerConRegister[factoryId] = br.ReadInt64();
                PowerChaRegister[factoryId] = br.ReadInt64();
                PowerDisRegister[factoryId] = br.ReadInt64();
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
                bw.Write(PowerGenRegister[factoryId]);
                bw.Write(PowerConRegister[factoryId]);
                bw.Write(PowerChaRegister[factoryId]);
                bw.Write(PowerDisRegister[factoryId]);
                bw.Write(EnergyStored[factoryId]);
                bw.Write(HashRegister[factoryId]);
            }
        }
    }
}
