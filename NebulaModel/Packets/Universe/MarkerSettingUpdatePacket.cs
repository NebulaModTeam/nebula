namespace NebulaModel.Packets.Universe;

public class MarkerSettingUpdatePacket
{
    public MarkerSettingUpdatePacket() { }

    public MarkerSettingUpdatePacket(int planetId, int markerId, MarkerSettingEvent settingEvent, int intValue = 0,
        float floatValue = 0f, string stringValue = null, short[] colorData = null)
    {
        PlanetId = planetId;
        MarkerId = markerId;
        Event = settingEvent;
        IntValue = intValue;
        FloatValue = floatValue;
        StringValue = stringValue;
        ColorData = colorData;
    }

    public int PlanetId { get; set; }
    public int MarkerId { get; set; }
    public MarkerSettingEvent Event { get; set; }
    public int IntValue { get; set; }
    public float FloatValue { get; set; }
    public string StringValue { get; set; }
    public short[] ColorData { get; set; }
}

public enum MarkerSettingEvent
{
    SetName = 0,
    SetWord = 1,
    SetTags = 2,
    SetTodoContent = 3,
    SetIcon = 4,
    SetColor = 5,
    SetVisibility = 6,
    SetDetailLevel = 7,
    SetHeight = 8,
    SetRadius = 9
}
