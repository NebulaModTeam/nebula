using System.Collections.Generic;
using NebulaAPI;
using NebulaAPI.Packets;

namespace NebulaModel.Packets.Factory.Storage;

public struct StorageSetFilterDataUpdate
{
    public int planetId;
    public int storageIndex;
    public EStorageType storageType;
    public int gridIndex;
    public int filterId;
}

public class StorageSetFilterData
{
    public StorageSetFilterData()
    {
    }

    public EStorageType StorageType { get; set; }
    public Dictionary<int, int> GridFilters { get; set; } = new();
}

public class StorageFilterSyncPacket : IDeferredPacket<StorageSetFilterDataUpdate>
{
    public int PlanetId { get; set; }

    // <StorageId, FilterData>
    public Dictionary<int, StorageSetFilterData> StorageFilters { get; set; } = [];

    public void UpdatePacket(StorageSetFilterDataUpdate data)
    {
        PlanetId = data.planetId;

        // Add a new storage if it doesn't already exist
        if (!StorageFilters.ContainsKey(data.storageIndex))
            StorageFilters.Add(
                data.storageIndex,
                new() { StorageType = data.storageType }
            );

        // Update filter
        var storage = StorageFilters[data.storageIndex];
        storage.GridFilters[data.gridIndex] = data.filterId;
    }

    public void Reset()
    {
    }
}
