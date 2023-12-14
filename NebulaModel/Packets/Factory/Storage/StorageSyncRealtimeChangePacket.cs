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
        bool useBan)
    {
        StorageEvent = storageEvent;
        ItemId = itemId;
        Count = count;
        UseBan = useBan;
        StorageIndex = storageIndex;
        PlanetId = GameMain.localPlanet?.id ?? -1;
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

    public StorageSyncRealtimeChangeEvent StorageEvent { get; }
    public int ItemId { get; }
    public int Count { get; }
    public bool UseBan { get; }
    public int StartIndex { get; }
    public int Length { get; }
    public int[] Needs { get; }
    public int StorageIndex { get; }
    public int Inc { get; }
    public int PlanetId { get; }
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
