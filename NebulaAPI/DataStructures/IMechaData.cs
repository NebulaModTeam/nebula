// unset

namespace NebulaAPI
{
    public interface IMechaData : INetSerializable
    {
        int SandCount { get; set; }
        double CoreEnergy { get; set; }
        double ReactorEnergy { get; set; }
        StorageComponent Inventory { get; set; }
        StorageComponent ReactorStorage { get; set; }
        StorageComponent WarpStorage { get; set; }
        MechaForge Forge { get; set; }
        int ResearchHashRate { get; set; }
    }
}