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

    public int PlanetId { get; }
    public int SpeakerId { get; }
    public SpeakerSettingEvent Event { get; }
    public int Parameter1 { get; }
    public int Parameter2 { get; }
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
