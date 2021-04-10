using UnityEngine;
using NebulaModel.DataStructures;

namespace NebulaModel.Packets.Logistics
{
    public class ILSRequestShipDock
    {
        public int stationGId { get; set; }
        public Float3 shipDockPos { get; set; }
        public Float4 shipDockRot { get; set; }
        public ILSRequestShipDock() { }
        public ILSRequestShipDock(int stationGId, Vector3 shipDockPos, Quaternion shipDockRot)
        {
            this.stationGId = stationGId;
            this.shipDockPos = new Float3(shipDockPos);
            this.shipDockRot = new Float4(shipDockRot);
        }
    }
}
