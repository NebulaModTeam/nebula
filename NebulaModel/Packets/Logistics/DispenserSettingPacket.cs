namespace NebulaModel.Packets.Logistics
{
    public class DispenserSettingPacket
    {
        public int PlanetId { get; set; }
        public int DispenserId { get; set; }
        public EDispenserSettingEvent Event { get; set; }
        public int Parameter1 { get; set; }

        public DispenserSettingPacket() { }
        public DispenserSettingPacket(int planetId, int dispenserId, EDispenserSettingEvent settingEvent, int parameter1)
        {
            PlanetId = planetId;
            DispenserId = dispenserId;
            Event = settingEvent;
            Parameter1 = parameter1;
        }
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
}
