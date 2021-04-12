using System;
using UnityEngine;
using NebulaModel.DataStructures;

namespace NebulaHost.PacketProcessors.Logistics
{
    // this one is only used when a client connects to sync all existing gStations.
    // needed because we also need to tell clients about ships that are already flying
    class ILSgStationPoolSync
    {
        public int[] stationGId { get; set; }
        public Float3[] DockPos { get; set; }
        public Float4[] DockRot { get; set; }
        public int[] shipStationGId { get; set; }
        public int[] shipStage { get; set; }
        public int[] shipDirection { get; set; }
        public int[] shipItemID { get; set; }
        public int[] shipItemCount { get; set; }
        public int[] shipPlanetA { get; set; }
        public int[] shipPlanetB { get; set; }
        public int[] shipIndex { get; set; }

        public ILSgStationPoolSync() { }
        public ILSgStationPoolSync(int[] stationGId, Float3[] DockPos, Float4[] DockRot, int[] shipStationGId, int[] shipStage, int[] shipDirection, int[] shipItemID, int[] shipItemCount, int[] shipPlanetA, int[] shipPlanetB, int[] shipIndex)
        {
            Array.Copy(stationGId, this.stationGId, stationGId.Length);
            Array.Copy(DockPos, this.DockPos, DockPos.Length);
            Array.Copy(DockRot, this.DockRot, DockRot.Length);
            Array.Copy(shipStationGId, this.shipStationGId, shipStationGId.Length);
            Array.Copy(shipStage, this.shipStage, shipStage.Length);
            Array.Copy(shipDirection, this.shipDirection, shipDirection.Length);
            Array.Copy(shipItemID, this.shipItemID, shipItemID.Length);
            Array.Copy(shipItemCount, this.shipItemCount, shipItemCount.Length);
            Array.Copy(shipPlanetA, this.shipPlanetA, shipPlanetA.Length);
            Array.Copy(shipPlanetB, this.shipPlanetB, shipPlanetB.Length);
            Array.Copy(shipIndex, this.shipIndex, shipIndex.Length);
        }
    }
}
