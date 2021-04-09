using UnityEngine;
using NebulaModel.DataStructures;

namespace NebulaModel.Packets.Logistics
{
    public class ILSRequestShipDockPos
    {
        public int stationGId { get; set; }
        public Float3 shipDockPos { get; set; }
        public ILSRequestShipDockPos() { }
        public ILSRequestShipDockPos(int stationGId, Vector3 shipDockPos)
        {
            this.stationGId = stationGId;
            this.shipDockPos = new Float3(shipDockPos);
        }
    }
}
