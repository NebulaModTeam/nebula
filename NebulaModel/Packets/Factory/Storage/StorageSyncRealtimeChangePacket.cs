namespace NebulaModel.Packets.Factory.Storage;

public class StorageSyncRealtimeChangePacket
{
    public StorageSyncRealtimeChangePacket() { }

    public StorageSyncRealtimeChangePacket(int storageIndex, StorageSyncRealtimeChangeEvent storageEvent, int itemId, int count,
        int inc)
    {
        StorageEvent = storageEvent;
        ItemId = itemId;
        Count = count;
        StorageIndex = storageIndex;
        Inc = inc;
        PlanetId = GameMain.localPlanet?.id ?? -1;
    }

    public StorageSyncRealtimeChangePacket(int storageIndex, StorageSyncRealtimeChangeEvent storageEvent, int itemId, int count,
        bool useBan, int planetId)
    {
        StorageEvent = storageEvent;
        ItemId = itemId;
        Count = count;
        UseBan = useBan;
        StorageIndex = storageIndex;
        PlanetId = planetId;
    }

    public StorageSyncRealtimeChangePacket(int storageIndex, StorageSyncRealtimeChangeEvent storageEvent, int itemId, int count,
        int[] needs, bool useBan)
    {
        StorageEvent = storageEvent;
        ItemId = itemId;
        Count = count;
        Needs = needs;
        UseBan = useBan;
        StorageIndex = storageIndex;
        PlanetId = GameMain.localPlanet?.id ?? -1;
    }

    public StorageSyncRealtimeChangePacket(int storageIndex, StorageSyncRealtimeChangeEvent storageEvent, int gridIndex,
#pragma warning disable IDE0060
        int itemId, int count, int inc)
#pragma warning restore IDE0060
    {
        StorageEvent = storageEvent;
        ItemId = itemId;
        Count = count;
        Length = gridIndex;
        StorageIndex = storageIndex;
        PlanetId = GameMain.localPlanet?.id ?? -1;
    }

    public StorageSyncRealtimeChangePacket(int storageIndex, StorageSyncRealtimeChangeEvent storageEvent, int itemId, int count,
        int startIndex, int length, int inc)
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
}

public enum StorageSyncRealtimeChangeEvent
{
    AddItem1 = 1,
    AddItem2 = 2,
    AddItemStacked = 3,
    AddItemFiltered = 4,
    TakeItem = 5,
    TakeItemFromGrid = 6,
    TakeHeadItems = 7,
    TakeTailItems1 = 8,
    TakeTailItems2 = 9
}
