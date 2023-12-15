namespace NebulaModel.Packets.Factory.Tank;

public class TankInputOutputSwitchPacket
{
    public TankInputOutputSwitchPacket() { }

    public TankInputOutputSwitchPacket(int tankIndex, bool isInput, bool inClosed, int planetId)
    {
        TankIndex = tankIndex;
        IsInput = isInput;
        IsClosed = inClosed;
        PlanetId = planetId;
    }

    public int TankIndex { get; set; }
    public bool IsInput { get; set; }
    public bool IsClosed { get; set; }
    public int PlanetId { get; set; }
}
