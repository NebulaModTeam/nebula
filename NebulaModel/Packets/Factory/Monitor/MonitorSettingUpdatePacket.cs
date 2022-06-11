namespace NebulaModel.Packets.Factory.Monitor
{
    public class MonitorSettingUpdatePacket
    {
        public int PlanetId { get; set; }
        public int MonitorId { get; set; }
        public MonitorSettingEvent Event { get; set; }
        public int Parameter1 { get; set; }
        public int Parameter2 { get; set; }

        public MonitorSettingUpdatePacket() { }
        public MonitorSettingUpdatePacket(int planetId, int monitorId, MonitorSettingEvent settingEvent, int parameter1, int parameter2)
        {
            PlanetId = planetId;
            MonitorId = monitorId;
            Event = settingEvent;
            Parameter1 = parameter1;
            Parameter2 = parameter2;
        }
        public MonitorSettingUpdatePacket(int planetId, int monitorId, MonitorSettingEvent settingEvent, int parameter)
            : this(planetId, monitorId, settingEvent, parameter, 0) { }
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
}
