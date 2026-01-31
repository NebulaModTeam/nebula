namespace NebulaModel.Packets.Factory.Monitor;

public class MonitorSettingUpdatePacket
{
    public MonitorSettingUpdatePacket() { }

    public MonitorSettingUpdatePacket(int planetId, int monitorId, MonitorSettingEvent settingEvent, int parameter1,
        int parameter2 = 0)
    {
        PlanetId = planetId;
        MonitorId = monitorId;
        Event = settingEvent;
        Parameter1 = parameter1;
        Parameter2 = parameter2;
    }

    public int PlanetId { get; set; }
    public int MonitorId { get; set; }
    public MonitorSettingEvent Event { get; set; }
    public int Parameter1 { get; set; }
    public int Parameter2 { get; set; }
}

public enum MonitorSettingEvent
{
    SetPassColorId = 0,
    SetFailColorId = 1,
    SetPassOperator = 2,
    SetMonitorMode = 3,
    SetSystemWarningMode = 4,
    SetSystemWarningSignalId = 5,
    SetDigitalSignalId = 6,
    SetCargoFilter = 7,
    SetTargetCargoBytes = 8,
    SetPeriodTickCount = 9,
    SetTargetBelt = 10,
    SetSpawnOperator = 11
}
