#region

using NebulaAPI.DataStructures;
using UnityEngine;

#endregion

namespace NebulaModel.Packets.Logistics;

public class ILSShipDock
{
    public ILSShipDock() { }

    // Update ship position and rotation with the host values
    // as they are computed based on dock pos and rot
    // and may be computed wrong before client received that information
    // so we correct that here
    public ILSShipDock(int stationGId, Vector3 shipDockPos, Quaternion shipDockRot, int[] shipOtherGId, int[] shipIndex,
        Double3[] shipPos, Float4[] shipRot, Double3[] shipPPosTemp, Float4[] shipPRotTemp)
    {
        this.stationGId = stationGId;
        this.shipDockPos = new Float3(shipDockPos);
        this.shipDockRot = new Float4(shipDockRot);

        this.shipOtherGId = shipOtherGId;
        this.shipIndex = shipIndex;
        this.shipPos = shipPos;
        this.shipRot = shipRot;
        this.shipPPosTemp = shipPPosTemp;
        this.shipPRotTemp = shipPRotTemp;
    }

    public int stationGId { get; }
    public Float3 shipDockPos { get; }
    public Float4 shipDockRot { get; }

    public int[] shipOtherGId { get; } // this is the GId of the station the ship belongs to (as stationGId points to the station that the ship has as otherGId)

    public int[] shipIndex { get; }
    public Double3[] shipPos { get; }
    public Float4[] shipRot { get; }
    public Double3[] shipPPosTemp { get; }
    public Float4[] shipPRotTemp { get; }
}
