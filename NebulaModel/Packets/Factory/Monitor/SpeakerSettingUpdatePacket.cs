namespace NebulaModel.Packets.Factory.Monitor;

public class SpeakerSettingUpdatePacket
{
    public SpeakerSettingUpdatePacket() { }

    public SpeakerSettingUpdatePacket(int planetId, int speakerId, SpeakerSettingEvent settingEvent, int parameter1,
        int parameter2 = 0)
    {
        PlanetId = planetId;
        SpeakerId = speakerId;
        Event = settingEvent;
        Parameter1 = parameter1;
        Parameter2 = parameter2;
    }

    public int PlanetId { get; set; }
    public int SpeakerId { get; set; }
    public SpeakerSettingEvent Event { get; set; }
    public int Parameter1 { get; set; }
    public int Parameter2 { get; set; }
}

public enum SpeakerSettingEvent
{
    SetTone = 0,
    SetVolume = 1,
    SetPitch = 2,
    SetLength = 3,
    SetRepeat = 4,
    SetFalloffRadius = 5
}
