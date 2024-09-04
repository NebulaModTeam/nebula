namespace NebulaModel.Packets.Logistics.ControlPanel;

public class LCPObjectEntryEntityInfo
{
    public LCPObjectEntryEntityInfo() { }

    public void Set(int index, int protoId, int id, string name)
    {
        Index = index;
        ProtoId = protoId;
        Id = id;
        Name = name;
    }

    public static readonly LCPObjectEntryEntityInfo Instance = new();

    public int Index { get; set; }
    public int ProtoId { get; set; }
    public int Id { get; set; }
    public string Name { get; set; }
}
