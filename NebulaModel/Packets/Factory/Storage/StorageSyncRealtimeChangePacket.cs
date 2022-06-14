namespace NebulaModel.Packets.Factory.Storage
{
    public class StorageSyncRealtimeChangePacket
    {
        public StorageSyncRealtimeChangeEvent StorageEvent { get; set; }
        public int ItemId { get; set; }
        public int Count { get; set; }
        public bool UseBan { get; set; }
        public int StartIndex { get; set; }
        public int Length { get; set; }
        public int[] Needs { get; set; }
        public int StorageIndex { get; set; }
        public int Inc { get; set; }
        public int PlanetId { get; set; }

        public StorageSyncRealtimeChangePacket() { }

        public StorageSyncRealtimeChangePacket(int storageIndex, StorageSyncRealtimeChangeEvent storageEvent, int itemId, int count, int inc)
        {
            StorageEvent = storageEvent;
            ItemId = itemId;
            Count = count;
            StorageIndex = storageIndex;
            Inc = inc;
            PlanetId = GameMain.localPlanet?.id ?? -1;
        }

        public StorageSyncRealtimeChangePacket(int storageIndex, StorageSyncRealtimeChangeEvent storageEvent, int itemId, int count, bool useBan)
        {
            StorageEvent = storageEvent;
            ItemId = itemId;
            Count = count;
            UseBan = useBan;
            StorageIndex = storageIndex;
            PlanetId = GameMain.localPlanet?.id ?? -1;
        }

        public StorageSyncRealtimeChangePacket(int storageIndex, StorageSyncRealtimeChangeEvent storageEvent, int itemId, int count, int[] needs, bool useBan)
        {
            StorageEvent = storageEvent;
            ItemId = itemId;
            Count = count;
            Needs = needs;
            UseBan = useBan;
            StorageIndex = storageIndex;
            PlanetId = GameMain.localPlanet?.id ?? -1;
        }

        public StorageSyncRealtimeChangePacket(int storageIndex, StorageSyncRealtimeChangeEvent storageEvent, int gridIndex, int itemId, int count, int inc)
        {
            StorageEvent = storageEvent;
            ItemId = itemId;
            Count = count;
            Length = gridIndex;
            StorageIndex = storageIndex;
            PlanetId = GameMain.localPlanet?.id ?? -1;
        }

        public StorageSyncRealtimeChangePacket(int storageIndex, StorageSyncRealtimeChangeEvent storageEvent, int itemId, int count, int startIndex, int length, int inc)
        {
            StorageEvent = storageEvent;
            ItemId = itemId;
            Count = count;
            StartIndex = startIndex;
            Length = length;
            StorageIndex = storageIndex;
            Inc = inc;
            PlanetId = GameMain.localPlanet?.id ?? -1;
        }
    }

    public enum StorageSyncRealtimeChangeEvent
    {
        AddItem1 = 1,
        AddItem2 = 2,
        AddItemStacked = 3,
        TakeItem = 4,
        TakeItemFromGrid = 5,
        TakeHeadItems = 6,
        TakeTailItems1 = 7,
        TakeTailItems2 = 8
    }
}
