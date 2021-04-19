using UnityEngine;
using NebulaModel.DataStructures;

namespace NebulaModel.Packets.Logistics
{
    public class ILSAddStationComponentRequest
    {
        public Float3 shipDockPos { get; set; }
        public int planetId { get; set; }
        public ILSAddStationComponentRequest() { }
        public ILSAddStationComponentRequest(int planetId, Vector3 shipDockPos)
        {
            this.planetId = planetId;
            this.shipDockPos = new Float3(shipDockPos);
        }
    }
}
