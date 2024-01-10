#region

using NebulaAPI.Interfaces;

#endregion

namespace NebulaAPI.DataStructures;

public interface IMechaData : INetSerializable
{
    long SandCount { get; set; }
    double CoreEnergy { get; set; }
    double ReactorEnergy { get; set; }
    StorageComponent Inventory { get; set; }
    DeliveryPackage DeliveryPackage { get; set; }
    StorageComponent ReactorStorage { get; set; }
    StorageComponent WarpStorage { get; set; }
    MechaForge Forge { get; set; }
    ConstructionModuleComponent ConstructionModule { get; set; }
    IMechaFightData FightData { get; set; }

    public void UpdateMech(Player destination);
}
