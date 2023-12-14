#region

using System.Diagnostics.CodeAnalysis;
using NebulaAPI.DataStructures;

#endregion

namespace NebulaModel.Packets.Logistics;

// this one is only used when a client connects to sync all existing gStations.
// needed because we also need to tell clients about ships that are already flying
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Field Name")]
public class ILSgStationPoolSync
{
    public ILSgStationPoolSync() { }

    public ILSgStationPoolSync(int[] stationGId,
        int[] stationMaxShipCount,
        int[] stationId,
        string[] stationName,
        Float3[] DockPos,
        Float4[] DockRot,
        int[] planetId,
        int[] workShipCount,
        int[] idleShipCount,
        ulong[] workShipIndices,
        ulong[] idleShipIndices,
        int[] shipStage,
        int[] shipDirection,
        float[] shipWarpState,
        int[] shipWarperCnt,
        int[] shipItemID,
        int[] shipItemCount,
        int[] shipPlanetA,
        int[] shipPlanetB,
        int[] shipOtherGId,
        float[] shipT,
        int[] shipIndex,
        Double3[] shipPos,
        Float4[] shipRot,
        Float3[] shipVel,
        float[] shipSpeed,
        Float3[] shipAngularVel,
        Double3[] shipPPosTemp,
        Float4[] shipPRotTemp)
    {
        this.stationGId = stationGId;
        this.stationMaxShipCount = stationMaxShipCount;
        this.stationId = stationId;
        this.stationName = stationName;
        this.DockPos = DockPos;
        this.DockRot = DockRot;
        this.planetId = planetId;
        this.workShipCount = workShipCount;
        this.idleShipCount = idleShipCount;
        this.workShipIndices = workShipIndices;
        this.idleShipIndices = idleShipIndices;
        this.shipStage = shipStage;
        this.shipDirection = shipDirection;
        this.shipWarpState = shipWarpState;
        this.shipWarperCnt = shipWarperCnt;
        this.shipItemID = shipItemID;
        this.shipItemCount = shipItemCount;
        this.shipPlanetA = shipPlanetA;
        this.shipPlanetB = shipPlanetB;
        this.shipOtherGId = shipOtherGId;
        this.shipT = shipT;
        this.shipIndex = shipIndex;
        this.shipPos = shipPos;
        this.shipRot = shipRot;
        this.shipVel = shipVel;
        this.shipSpeed = shipSpeed;
        this.shipAngularVel = shipAngularVel;
        this.shipPPosTemp = shipPPosTemp;
        this.shipPRotTemp = shipPRotTemp;
    }

    public int[] stationGId { get; }

    public int[] stationMaxShipCount { get; }
    public int[] stationId { get; }
    public string[] stationName { get; }
    public Float3[] DockPos { get; }
    public Float4[] DockRot { get; }
    public int[] planetId { get; }
    public int[] workShipCount { get; }
    public int[] idleShipCount { get; }
    public ulong[] workShipIndices { get; }
    public ulong[] idleShipIndices { get; }
    public int[] shipStage { get; }
    public int[] shipDirection { get; }
    public float[] shipWarpState { get; }
    public int[] shipWarperCnt { get; }
    public int[] shipItemID { get; }
    public int[] shipItemCount { get; }
    public int[] shipPlanetA { get; }
    public int[] shipPlanetB { get; }
    public int[] shipOtherGId { get; }
    public float[] shipT { get; }
    public int[] shipIndex { get; }
    public Double3[] shipPos { get; }
    public Float4[] shipRot { get; }
    public Float3[] shipVel { get; }
    public float[] shipSpeed { get; }
    public Float3[] shipAngularVel { get; }
    public Double3[] shipPPosTemp { get; }
    public Float4[] shipPRotTemp { get; }
}
