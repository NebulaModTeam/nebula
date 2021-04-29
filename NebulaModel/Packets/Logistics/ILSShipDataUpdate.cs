using System;

namespace NebulaModel.Packets.Logistics
{
    public class ILSShipDataUpdate
    {
        public int stationGId { get; set; }
        public int[] shipIndex { get; set; }
        public int[] otherGId { get; set; }
        public int[] direction { get; set; }
        public int[] itemId { get; set; }

        public ILSShipDataUpdate() { }
        public ILSShipDataUpdate(int stationGId, int[] shipIndex, int[] otherGId, int[] direction, int[] itemId)
        {
            this.shipIndex = new int[shipIndex.Length];
            this.otherGId = new int[otherGId.Length];
            this.direction = new int[direction.Length];
            this.itemId = new int[itemId.Length];

            this.stationGId = stationGId;
            Array.Copy(shipIndex, this.shipIndex, shipIndex.Length);
            Array.Copy(otherGId, this.otherGId, otherGId.Length);
            Array.Copy(direction, this.direction, direction.Length);
            Array.Copy(itemId, this.itemId, itemId.Length);
        }
    }
}
