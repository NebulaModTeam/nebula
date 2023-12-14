namespace NebulaModel.Packets.Factory.PowerExchanger;

public class PowerExchangerChangeModePacket
{
    public PowerExchangerChangeModePacket() { }

    public PowerExchangerChangeModePacket(int powerExchangerIndex, int mode, int planetId)
    {
        PowerExchangerIndex = powerExchangerIndex;
        Mode = mode;
        PlanetId = planetId;
    }

    public int PowerExchangerIndex { get; set; }
    public int Mode { get; set; }
    public int PlanetId { get; set; }
}
