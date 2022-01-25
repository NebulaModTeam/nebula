namespace NebulaModel.Packets.Logistics
{
    public class ILSShipEnterWarp
    {
        public int ThisGId { get; set; }
        public int WorkShipIndex { get; set; }
        public ILSShipEnterWarp() { }
        public ILSShipEnterWarp(int thisGId, int workShipIndex)
        {
            ThisGId = thisGId;
            WorkShipIndex = workShipIndex;
        }
    }
}
