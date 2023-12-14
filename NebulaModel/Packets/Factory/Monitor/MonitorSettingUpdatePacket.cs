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

    public int PlanetId { get; }
    public int MonitorId { get; }
    public MonitorSettingEvent Event { get; }
    public int Parameter1 { get; }
    public int Parameter2 { get; }
}

public enum MonitorSettingEvent
{
    SetPassColorId = 0,
    SetFailColorId = 1,
    SetPassOperator = 2,
    SetMonitorMode = 3,
    SetSystemWarningMode = 4,
    SetSystemWarningSignalId = 5,
    SetCargoFilter = 6,
    SetTargetCargoBytes = 7,
    SetPeriodTickCount = 8,
    SetTargetBelt = 9,
    SetSpawnOperator = 10
}
