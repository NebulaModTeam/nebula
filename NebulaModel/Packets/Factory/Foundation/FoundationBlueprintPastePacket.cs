using System.Collections.Generic;

namespace NebulaModel.Packets.Factory.Foundation;

public class FoundationBlueprintPastePacket
{
    public FoundationBlueprintPastePacket() { }

    public FoundationBlueprintPastePacket(int planetId, List<int> reformGridIds,
        Dictionary<int, int> levelChanges, int reformType, int reformColor)
    {
        PlanetId = planetId;
        ReformGridIds = reformGridIds;
        LevelChanges = levelChanges;
        ReformType = reformType;
        ReformColor = reformColor;
    }

    public int PlanetId { get; set; }
    public List<int> ReformGridIds { get; set; }
    public Dictionary<int, int> LevelChanges { get; set; }
    public int ReformType { get; set; }
    public int ReformColor { get; set; }
}
