using System;
using System.Collections.Generic;

namespace NebulaModel.Packets.Factory.Foundation;

public class FoundationBlueprintPastePacket
{
    public FoundationBlueprintPastePacket() { }

    public FoundationBlueprintPastePacket(int planetId, Dictionary<int, int> levelChanges,
        byte[] reformData)
    {
        PlanetId = planetId;
        LevelChangesKeys = new int[levelChanges.Count];
        LevelChangesValues = new int[levelChanges.Count];
        var index = 0;
        foreach (var pair in levelChanges)
        {
            LevelChangesKeys[index] = pair.Key;
            LevelChangesValues[index] = pair.Value;
            index++;
        }
        ReformData = reformData;
        ReformGridIds = [];
    }

    public FoundationBlueprintPastePacket(int planetId, Dictionary<int, int> levelChanges,
        int[] reformGridIds, int reformType, int reformColor)
    {
        PlanetId = planetId;
        LevelChangesKeys = new int[levelChanges.Count];
        LevelChangesValues = new int[levelChanges.Count];
        var index = 0;
        foreach (var pair in levelChanges)
        {
            LevelChangesKeys[index] = pair.Key;
            LevelChangesValues[index] = pair.Value;
            index++;
        }
        ReformData = [];
        ReformGridIds = reformGridIds;
        ReformType = reformType;
        ReformColor = reformColor;
    }

    public int PlanetId { get; set; }
    public int[] LevelChangesKeys { get; set; }
    public int[] LevelChangesValues { get; set; }
    public byte[] ReformData { get; set; }
    public int[] ReformGridIds { get; set; }
    public int ReformType { get; set; }
    public int ReformColor { get; set; }
}
