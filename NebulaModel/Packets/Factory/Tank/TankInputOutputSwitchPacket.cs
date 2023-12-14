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

    public int TankIndex { get; }
    public bool IsInput { get; }
    public bool IsClosed { get; }
    public int PlanetId { get; }
}
