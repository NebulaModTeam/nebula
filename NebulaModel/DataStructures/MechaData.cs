#region

using System.IO;
using NebulaAPI.DataStructures;
using NebulaAPI.Interfaces;
using NebulaAPI.Packets;
using static NebulaModel.Networking.BinaryUtils;

#endregion

namespace NebulaModel.DataStructures;

[RegisterNestedType]
public class MechaData : IMechaData
{
    public MechaData()
    {
        // This is needed for the serialization and deserialization
        Forge = new MechaForge { tasks = [] };
        TechBonuses = new PlayerTechBonuses();
    }

    public MechaData(Player player)
    {
        SandCount = player.sandCount;
        CoreEnergy = player.mecha.coreEnergy;
        ReactorEnergy = player.mecha.reactorEnergy;
        ReactorStorage = player.mecha.reactorStorage;
        WarpStorage = player.mecha.warpStorage;
        Forge = player.mecha.forge;
        Inventory = player.package;
        DeliveryPackage = player.deliveryPackage;
        ConstructionModule = player.mecha.constructionModule;
        FightData = new MechaFightData(player);
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
    public ConstructionModuleComponent ConstructionModule { get; set; }
    public IMechaFightData FightData { get; set; }

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
        FightData.Serialize(writer);
        using var ms = new MemoryStream();
        using (var wr = new BinaryWriter(ms))
        {
            Inventory.Export(wr);
            DeliveryPackage.Export(wr);
            ReactorStorage.Export(wr);
            WarpStorage.Export(wr);
            Forge.Export(wr);
            ConstructionModule.Export(wr);
        }
        var export = ms.ToArray();
        writer.Put(export.Length);
        writer.Put(export);
    }

    public void Deserialize(INetDataReader reader)
    {
        TechBonuses = new PlayerTechBonuses();
        FightData = new MechaFightData();
        Inventory = new StorageComponent(4);
        DeliveryPackage = new DeliveryPackage();
        DeliveryPackage.Init();
        ReactorStorage = new StorageComponent(4);
        WarpStorage = new StorageComponent(1);
        Forge = new MechaForge { tasks = [], extraItems = new ItemBundle() };
        ConstructionModule = new ConstructionModuleComponent();
        TechBonuses.Deserialize(reader);
        SandCount = reader.GetLong();
        CoreEnergy = reader.GetDouble();
        ReactorEnergy = reader.GetDouble();
        var isPayloadPresent = reader.GetBool();
        if (!isPayloadPresent)
        {
            return;
        }
        FightData.Deserialize(reader);
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
        ConstructionModule.Import(br);
    }

    public void UpdateMech(Player destination)
    {
        destination.package = Inventory;
        using (var ms = new MemoryStream())
        {
            var bw = new BinaryWriter(ms);
            DeliveryPackage.Export(bw);
            ms.Seek(0, SeekOrigin.Begin);
            var br = new BinaryReader(ms);
            destination.deliveryPackage.Import(br);
            DeliveryPackage = destination.deliveryPackage;
        }
        destination.mecha.coreEnergy = CoreEnergy;
        destination.mecha.reactorEnergy = ReactorEnergy;
        destination.mecha.forge = Forge;
        destination.mecha.reactorStorage = ReactorStorage;
        destination.mecha.warpStorage = WarpStorage;
        destination.mecha.constructionModule = ConstructionModule;
        FightData.UpdateMech(destination);
        destination.SetSandCount(SandCount);
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
        ConstructionModule = new ConstructionModuleComponent();
        TechBonuses.Import(reader, revision);
        SandCount = reader.GetInt();
        CoreEnergy = reader.GetDouble();
        ReactorEnergy = reader.GetDouble();
        var isPayloadPresent = reader.GetBool();
        if (!isPayloadPresent)
        {
            return;
        }
        if (revision >= 8)
        {
            FightData.Deserialize(reader);
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
        if (revision >= 8)
        {
            ConstructionModule.Import(br);
        }
    }
}
