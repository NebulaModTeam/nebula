namespace NebulaModel.Packets.Factory.BattleBase;

public class BattleBaseSettingUpdatePacket
{
    public BattleBaseSettingUpdatePacket() { }

    public BattleBaseSettingUpdatePacket(int planetId, int battleBaseId, BattleBaseSettingEvent settingEvent, float arg1)
    {
        PlanetId = planetId;
        BattleBaseId = battleBaseId;
        Event = settingEvent;
        Arg1 = arg1;
    }

    public int PlanetId { get; set; }
    public int BattleBaseId { get; set; }
    public BattleBaseSettingEvent Event { get; set; }
    public float Arg1 { get; set; }
}

public enum BattleBaseSettingEvent
{
    None = 0,
    ChangeMaxChargePower = 1, //OnMaxChargePowerSliderChange
    ToggleDroneEnabled = 2, //OnDroneButtonClick
    ChangeDronesPriority = 3, //OnDroneButtonClick
    ToggleCombatModuleEnabled = 4, //OnFleetButtonClick
    ToggleAutoReconstruct = 5, //OnAutoReconstructButtonClick
    ToggleAutoPickEnabled = 6, //OnAutoPickButtonClick
    ChangeFleetConfig = 7, //OnFleetTypeButtonClick
    ToggleAutoReplenishFleet = 8 //OnAutoReplenishButtonClick
}
