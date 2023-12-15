#region

using System.Collections.Generic;
using System.IO;

#endregion

namespace NebulaModel.DataStructures;

public class StatisticalSnapShot
{
    public readonly long CapturedGameTick;
    public readonly long[] EnergyStored;
    public readonly long[] HashRegister;
    public readonly long[] PowerChargingRegister;
    public readonly long[] PowerConsumptionRegister;
    public readonly long[] PowerDischargingRegister;

    public readonly long[] PowerGenerationRegister;

    //List of statistical changes for each planet that happened in one gameTick
    public readonly List<ProductionChangeStruct>[] ProductionChangesPerFactory;

    public StatisticalSnapShot(long gameTick, int numOfActiveFactories)
    {
        ProductionChangesPerFactory = new List<ProductionChangeStruct>[numOfActiveFactories];
        for (var i = 0; i < numOfActiveFactories; i++)
        {
            ProductionChangesPerFactory[i] = [];
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
        var factoryCount = br.ReadInt32();

        ProductionChangesPerFactory = new List<ProductionChangeStruct>[factoryCount];
        PowerGenerationRegister = new long[factoryCount];
        PowerConsumptionRegister = new long[factoryCount];
        PowerChargingRegister = new long[factoryCount];
        PowerDischargingRegister = new long[factoryCount];
        EnergyStored = new long[factoryCount];
        HashRegister = new long[factoryCount];

        for (var factoryId = 0; factoryId < factoryCount; factoryId++)
        {
            ProductionChangesPerFactory[factoryId] = [];
            var changesCount = br.ReadInt32();
            for (var changeId = 0; changeId < changesCount; changeId++)
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
        bw.Write(CapturedGameTick);

        //Collect production/consumption statistics from factories
        bw.Write(ProductionChangesPerFactory.Length);
        for (var factoryId = 0; factoryId < ProductionChangesPerFactory.Length; factoryId++)
        {
            bw.Write(ProductionChangesPerFactory[factoryId].Count);
            for (var changeId = 0; changeId < ProductionChangesPerFactory[factoryId].Count; changeId++)
            {
                ProductionChangesPerFactory[factoryId][changeId].Export(bw);
            }

            //Collect info about power system of the factory
            bw.Write(PowerGenerationRegister[factoryId]);
            bw.Write(PowerConsumptionRegister[factoryId]);
            bw.Write(PowerChargingRegister[factoryId]);
            bw.Write(PowerDischargingRegister[factoryId]);
            bw.Write(EnergyStored[factoryId]);
            bw.Write(HashRegister[factoryId]);
        }
    }

    public readonly struct ProductionChangeStruct //12 bytes total
        (bool isProduction, ushort productId, int amount)
    {
        public readonly bool IsProduction = isProduction; //1-byte
        public readonly ushort ProductId = productId; //2-byte
        public readonly int Amount = amount; //4-byte

        public ProductionChangeStruct(BinaryReader r) : this(r.ReadBoolean(), r.ReadUInt16(), r.ReadInt32())
        {
        }

        public void Export(BinaryWriter w)
        {
            w.Write(IsProduction);
            w.Write(ProductId);
            w.Write(Amount);
        }
    }
}
