using UnityEngine;
using NebulaModel.DataStructures;
using System;

namespace NebulaModel.Packets.Logistics
{
    public class ILSShipDock
    {
        public int stationGId { get; set; }
        public Float3 shipDockPos { get; set; }
        public Float4 shipDockRot { get; set; }
        public int[] shipOtherGId { get; set; } // this is the GId of the station the ship belongs to (as stationGId points to the station that the ship has as otherGId)
        public int[] shipIndex { get; set; }
        public Double3[] shipPos { get; set; }
        public Float4[] shipRot { get; set; }
        public Double3[] shipPPosTemp { get; set; }
        public Float4[] shipPRotTemp { get; set; }
        public ILSShipDock() { }
        // Update ship position and rotation with the host values
        // as they are computed based on dock pos and rot
        // and may be computed wrong before client received that information
        // so we correct that here
        public ILSShipDock(int stationGId, Vector3 shipDockPos, Quaternion shipDockRot, int[] shipOtherGId, int[] shipIndex, Double3[] shipPos, Float4[] shipRot, Double3[] shipPPosTemp, Float4[] shipPRotTemp)
        {
            this.shipOtherGId = new int[shipOtherGId.Length];
            this.shipIndex = new int[shipIndex.Length];
            this.shipPos = new Double3[shipPos.Length];
            this.shipRot = new Float4[shipRot.Length];
            this.shipPPosTemp = new Double3[shipPPosTemp.Length];
            this.shipPRotTemp = new Float4[shipPRotTemp.Length];

            this.stationGId = stationGId;
            this.shipDockPos = new Float3(shipDockPos);
            this.shipDockRot = new Float4(shipDockRot);

            Array.Copy(shipOtherGId, this.shipOtherGId, shipOtherGId.Length);
            Array.Copy(shipIndex, this.shipIndex, shipIndex.Length);
            Array.Copy(shipPos, this.shipPos, shipPos.Length);
            Array.Copy(shipRot, this.shipRot, shipRot.Length);
            Array.Copy(shipPPosTemp, this.shipPPosTemp, shipPPosTemp.Length);
            Array.Copy(shipPRotTemp, this.shipPRotTemp, shipPRotTemp.Length);
        }
    }
}
