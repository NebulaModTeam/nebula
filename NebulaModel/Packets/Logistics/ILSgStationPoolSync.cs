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

    public int[] stationGId { get; set; }

    public int[] stationMaxShipCount { get; set; }
    public int[] stationId { get; set; }
    public string[] stationName { get; set; }
    public Float3[] DockPos { get; set; }
    public Float4[] DockRot { get; set; }
    public int[] planetId { get; set; }
    public int[] workShipCount { get; set; }
    public int[] idleShipCount { get; set; }
    public ulong[] workShipIndices { get; set; }
    public ulong[] idleShipIndices { get; set; }
    public int[] shipStage { get; set; }
    public int[] shipDirection { get; set; }
    public float[] shipWarpState { get; set; }
    public int[] shipWarperCnt { get; set; }
    public int[] shipItemID { get; set; }
    public int[] shipItemCount { get; set; }
    public int[] shipPlanetA { get; set; }
    public int[] shipPlanetB { get; set; }
    public int[] shipOtherGId { get; set; }
    public float[] shipT { get; set; }
    public int[] shipIndex { get; set; }
    public Double3[] shipPos { get; set; }
    public Float4[] shipRot { get; set; }
    public Float3[] shipVel { get; set; }
    public float[] shipSpeed { get; set; }
    public Float3[] shipAngularVel { get; set; }
    public Double3[] shipPPosTemp { get; set; }
    public Float4[] shipPRotTemp { get; set; }
}
