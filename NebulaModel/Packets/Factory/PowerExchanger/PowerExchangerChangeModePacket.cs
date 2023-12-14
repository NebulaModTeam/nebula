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

    public int PowerExchangerIndex { get; }
    public int Mode { get; }
    public int PlanetId { get; }
}
