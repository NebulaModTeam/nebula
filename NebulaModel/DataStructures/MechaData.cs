#region

using System.IO;
using NebulaAPI.DataStructures;
using NebulaAPI.Interfaces;
using NebulaAPI.Packets;
using NebulaModel.Packets.Players;

#endregion

namespace NebulaModel.DataStructures;

[RegisterNestedType]
public class MechaData : IMechaData
{
    public MechaData()
    {
        //This is needed for the serialization and deserialization
        Forge = new MechaForge { tasks = [] };
        TechBonuses = new PlayerTechBonuses();
    }

    public MechaData(long sandCount, double coreEnergy, double reactorEnergy, StorageComponent inventory,
        DeliveryPackage deliveryPackage, StorageComponent reactorStorage, StorageComponent warpStorage, MechaForge forge)
    {
        SandCount = sandCount;
        CoreEnergy = coreEnergy;
        ReactorEnergy = reactorEnergy;
        ReactorStorage = reactorStorage;
        WarpStorage = warpStorage;
        Forge = forge;
        Inventory = inventory;
        DeliveryPackage = deliveryPackage;
        TechBonuses = new PlayerTechBonuses();
    }

    public PlayerTechBonuses TechBonuses { get; set; }
    public long SandCount { get; set; }
    public double CoreEnergy { get; set; }
    public double ReactorEnergy { get; set; }
    public StorageComponent Inventory { get; set; }
    public DeliveryPackage DeliveryPackage { get; set; }
    public StorageComponent ReactorStorage { get; set; }
    public StorageComponent WarpStorage { get; set; }
    public MechaForge Forge { get; set; }

    public void Serialize(INetDataWriter writer)
    {
        TechBonuses.Serialize(writer);
        writer.Put(SandCount);
        writer.Put(CoreEnergy);
        writer.Put(ReactorEnergy);
        writer.Put(ReactorStorage != null);
        if (ReactorStorage == null)
        {
            return;
        }
        using var ms = new MemoryStream();
        using (var wr = new BinaryWriter(ms))
        {
            Inventory.Export(wr);
            DeliveryPackage.Export(wr);
            ReactorStorage.Export(wr);
            WarpStorage.Export(wr);
            Forge.Export(wr);
        }
        var export = ms.ToArray();
        writer.Put(export.Length);
        writer.Put(export);
    }

    public void Deserialize(INetDataReader reader)
    {
        TechBonuses = new PlayerTechBonuses();
        Inventory = new StorageComponent(4);
        DeliveryPackage = new DeliveryPackage();
        DeliveryPackage.Init();
        ReactorStorage = new StorageComponent(4);
        WarpStorage = new StorageComponent(1);
        Forge = new MechaForge { tasks = [], extraItems = new ItemBundle() };
        TechBonuses.Deserialize(reader);
        SandCount = reader.GetInt();
        CoreEnergy = reader.GetDouble();
        ReactorEnergy = reader.GetDouble();
        var isPayloadPresent = reader.GetBool();
        if (!isPayloadPresent)
        {
            return;
        }
        var mechaLength = reader.GetInt();
        var mechaBytes = new byte[mechaLength];
        reader.GetBytes(mechaBytes, mechaLength);
        using var ms = new MemoryStream(mechaBytes);
        using var br = new BinaryReader(ms);
        Inventory.Import(br);
        DeliveryPackage.Import(br);
        ReactorStorage.Import(br);
        WarpStorage.Import(br);
        Forge.Import(br);
    }

    public void Import(INetDataReader reader, int revision)
    {
        TechBonuses = new PlayerTechBonuses();
        Inventory = new StorageComponent(4);
        DeliveryPackage = new DeliveryPackage();
        DeliveryPackage.Init();
        ReactorStorage = new StorageComponent(4);
        WarpStorage = new StorageComponent(1);
        Forge = new MechaForge { tasks = [], extraItems = new ItemBundle() };
        TechBonuses.Import(reader, revision);
        SandCount = reader.GetInt();
        CoreEnergy = reader.GetDouble();
        ReactorEnergy = reader.GetDouble();
        var isPayloadPresent = reader.GetBool();
        if (!isPayloadPresent)
        {
            return;
        }
        var mechaLength = reader.GetInt();
        var mechaBytes = new byte[mechaLength];
        reader.GetBytes(mechaBytes, mechaLength);
        using var ms = new MemoryStream(mechaBytes);
        using var br = new BinaryReader(ms);
        Inventory.Import(br);
        if (revision >= 7)
        {
            DeliveryPackage.Import(br);
        }
        ReactorStorage.Import(br);
        WarpStorage.Import(br);
        Forge.Import(br);
    }
}
