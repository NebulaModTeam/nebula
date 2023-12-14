namespace NebulaModel.Packets.Logistics;

public class ILSShipEnterWarp
{
    public ILSShipEnterWarp() { }

    public ILSShipEnterWarp(int thisGId, int workShipIndex)
    {
        ThisGId = thisGId;
        WorkShipIndex = workShipIndex;
    }

    public int ThisGId { get; }
    public int WorkShipIndex { get; }
}
