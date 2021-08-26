using NebulaAPI;
using NebulaModel.Packets.Players;
using System.Collections.Generic;
using System.IO;

namespace NebulaModel.DataStructures
{
    [RegisterNestedType]
    public class MechaData : INetSerializable
    {
        public int SandCount { get; set; }
        public double CoreEnergy { get; set; }
        public double ReactorEnergy { get; set; }
        public StorageComponent Inventory { get; set; }
        public StorageComponent ReactorStorage { get; set; }
        public StorageComponent WarpStorage { get; set; }
        public MechaForge Forge { get; set; }
        public PlayerTechBonuses TechBonuses { get; set; }

        public MechaData()
        {
            //This is needed for the serialization and deserialization
            this.Forge = new MechaForge
            {
                tasks = new List<ForgeTask>()
            };
            this.TechBonuses = new PlayerTechBonuses();
        }

        public MechaData(int sandCount, double coreEnergy, double reactorEnergy, StorageComponent inventory, StorageComponent reactorStorage, StorageComponent warpStorage, MechaForge forge)
        {
            this.SandCount = sandCount;
            this.CoreEnergy = coreEnergy;
            this.ReactorEnergy = reactorEnergy;
            this.ReactorStorage = reactorStorage;
            this.WarpStorage = warpStorage;
            this.Forge = forge;
            this.Inventory = inventory;
            this.TechBonuses = new PlayerTechBonuses();
        }

        public void Serialize(INetDataWriter writer)
        {
            TechBonuses.Serialize(writer);
            writer.Put(SandCount);
            writer.Put(CoreEnergy);
            writer.Put(ReactorEnergy);
            writer.Put(ReactorStorage != null);
            if (ReactorStorage != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter wr = new BinaryWriter(ms))
                    {
                        Inventory.Export(wr);
                        ReactorStorage.Export(wr);
                        WarpStorage.Export(wr);
                        Forge.Export(wr);
                    }
                    byte[] export = ms.ToArray();
                    writer.Put(export.Length);
                    writer.Put(export);
                }
            }
        }

        public void Deserialize(INetDataReader reader)
        {
            TechBonuses = new PlayerTechBonuses();
            Inventory = new StorageComponent(4);
            ReactorStorage = new StorageComponent(4);
            WarpStorage = new StorageComponent(1);
            Forge = new MechaForge
            {
                tasks = new List<ForgeTask>(),
                extraItems = new ItemPack()
            };
            TechBonuses.Deserialize(reader);
            SandCount = reader.GetInt();
            CoreEnergy = reader.GetDouble();
            ReactorEnergy = reader.GetDouble();
            bool isPayloadPresent = reader.GetBool();
            if (isPayloadPresent)
            {
                int mechaLength = reader.GetInt();
                byte[] mechaBytes = new byte[mechaLength];
                reader.GetBytes(mechaBytes, mechaLength);
                using (MemoryStream ms = new MemoryStream(mechaBytes))
                using (BinaryReader br = new BinaryReader(ms))
                {
                    Inventory.Import(br);
                    ReactorStorage.Import(br);
                    WarpStorage.Import(br);
                    Forge.Import(br);
                }
            }
        }
    }
}
