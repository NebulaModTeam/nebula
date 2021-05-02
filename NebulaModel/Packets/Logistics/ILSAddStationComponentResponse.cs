using UnityEngine;
using NebulaModel.DataStructures;

namespace NebulaModel.Packets.Logistics
{
    public class ILSAddStationComponentResponse
    {
        public int stationGId { get; set; }
        public int planetId { get; set; }
        public Float3 shipDockPos { get; set; }
        public ILSAddStationComponentResponse() { }
        public ILSAddStationComponentResponse(int stationGId, int planetId, Vector3 shipDockPos)
        {
            this.stationGId = stationGId;
            this.planetId = planetId;
            this.shipDockPos = new Float3(shipDockPos);
        }
    }
}
