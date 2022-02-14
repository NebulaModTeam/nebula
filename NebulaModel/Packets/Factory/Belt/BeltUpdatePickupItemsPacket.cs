using NebulaAPI;

namespace NebulaModel.Packets.Factory.Belt
{
    public class BeltUpdatePickupItemsPacket
    {
        public int PlanetId { get; set; }
        public BeltUpdate[] BeltUpdates { get; set; }

        public BeltUpdatePickupItemsPacket() { }

        public BeltUpdatePickupItemsPacket(BeltUpdate[] beltUpdates, int planetId)
        {
            BeltUpdates = beltUpdates;
            PlanetId = planetId;
        }
    }

    [RegisterNestedType]
    public struct BeltUpdate : INetSerializable
    {
        public int ItemId { get; set; }
        public int Count { get; set; }
        public int BeltId { get; set; }

        public BeltUpdate(int itemId, int count, int beltId)
        {
            ItemId = itemId;
            Count = count;
            BeltId = beltId;
        }

        public void Serialize(INetDataWriter writer)
        {
            writer.Put(ItemId);
            writer.Put(Count);
            writer.Put(BeltId);
        }

        public void Deserialize(INetDataReader reader)
        {
            ItemId = reader.GetInt();
            Count = reader.GetInt();
            BeltId = reader.GetInt();
        }
    }
}
