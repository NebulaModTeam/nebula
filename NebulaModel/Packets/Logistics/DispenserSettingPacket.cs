namespace NebulaModel.Packets.Logistics;

public class DispenserSettingPacket
{
    public DispenserSettingPacket() { }

    public DispenserSettingPacket(int planetId, int dispenserId, EDispenserSettingEvent settingEvent, int parameter1)
    {
        PlanetId = planetId;
        DispenserId = dispenserId;
        Event = settingEvent;
        Parameter1 = parameter1;
    }

    public int PlanetId { get; }
    public int DispenserId { get; }
    public EDispenserSettingEvent Event { get; }
    public int Parameter1 { get; }
}

public enum EDispenserSettingEvent
{
    None,
    SetCourierCount, // CourierIconClick
    ToggleAutoReplenish, // CourierAutoReplenishButtonClick
    SetMaxChargePower, // MaxChargePowerSliderValueChange
    SetFilter, // GuessFilterButtonClick, OnItemPickerReturn, OnTakeBackButtonClick
    SetPlayerDeliveryMode, // ModeSwitchClicked
    SetStorageDeliveryMode // ModeToggleClicked
}
