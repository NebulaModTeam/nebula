using System;
using UnityEngine;
using NebulaModel.DataStructures;

namespace NebulaModel.Packets.Logistics
{
    // this one is only used when a client connects to sync all existing gStations.
    // needed because we also need to tell clients about ships that are already flying
    public class ILSgStationPoolSync
    {
        public int[] stationGId { get; set; }
        public int[] stationId { get; set; }
        public Float3[] DockPos { get; set; }
        public Float4[] DockRot { get; set; }
        public int[] planetId { get; set; }
        public int[] workShipCount { get; set; }
        public int[] idleShipCount { get; set; }
        public ulong[] workShipIndices { get; set; }
        public ulong[] idleShipIndices { get; set; }
        public int[] shipStationGId { get; set; }
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

        public ILSgStationPoolSync() { }
        public ILSgStationPoolSync(int[] stationGId,
                                    int[] stationId,
                                    Float3[] DockPos,
                                    Float4[] DockRot,
                                    int[] planetId,
                                    int[] workShipCount,
                                    int[] idleShipCount,
                                    ulong[] workShipIndices,
                                    ulong[] idleShipIndices,
                                    int[] shipStationGId,
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
            this.stationGId = new int[stationGId.Length];
            this.stationId = new int[stationId.Length];
            this.DockPos = new Float3[DockPos.Length];
            this.DockRot = new Float4[DockRot.Length];
            this.planetId = new int[planetId.Length];
            this.workShipCount = new int[workShipCount.Length];
            this.idleShipCount = new int[idleShipCount.Length];
            this.workShipIndices = new ulong[workShipIndices.Length];
            this.idleShipIndices = new ulong[idleShipIndices.Length];
            this.shipStationGId = new int[shipStationGId.Length];
            this.shipStage = new int[shipStage.Length];
            this.shipDirection = new int[shipDirection.Length];
            this.shipWarpState = new float[shipWarpState.Length];
            this.shipWarperCnt = new int[shipWarperCnt.Length];
            this.shipItemID = new int[shipItemID.Length];
            this.shipItemCount = new int[shipItemCount.Length];
            this.shipPlanetA = new int[shipPlanetA.Length];
            this.shipPlanetB = new int[shipPlanetB.Length];
            this.shipOtherGId = new int[shipOtherGId.Length];
            this.shipT = new float[shipT.Length];
            this.shipIndex = new int[shipIndex.Length];
            this.shipPos = new Double3[shipPos.Length];
            this.shipRot = new Float4[shipRot.Length];
            this.shipVel = new Float3[shipVel.Length];
            this.shipSpeed = new float[shipSpeed.Length];
            this.shipAngularVel = new Float3[shipAngularVel.Length];
            this.shipPPosTemp = new Double3[shipPPosTemp.Length];
            this.shipPRotTemp = new Float4[shipPRotTemp.Length];

            Array.Copy(stationGId, this.stationGId, stationGId.Length);
            Array.Copy(stationId, this.stationId, stationId.Length);
            Array.Copy(DockPos, this.DockPos, DockPos.Length);
            Array.Copy(DockRot, this.DockRot, DockRot.Length);
            Array.Copy(planetId, this.planetId, planetId.Length);
            Array.Copy(workShipCount, this.workShipCount, workShipCount.Length);
            Array.Copy(idleShipCount, this.idleShipCount, idleShipCount.Length);
            Array.Copy(workShipIndices, this.workShipIndices, workShipIndices.Length);
            Array.Copy(idleShipIndices, this.idleShipIndices, idleShipIndices.Length);
            Array.Copy(shipStationGId, this.shipStationGId, shipStationGId.Length);
            Array.Copy(shipStage, this.shipStage, shipStage.Length);
            Array.Copy(shipDirection, this.shipDirection, shipDirection.Length);
            Array.Copy(shipWarpState, this.shipWarpState, shipWarpState.Length);
            Array.Copy(shipWarperCnt, this.shipWarperCnt, shipWarperCnt.Length);
            Array.Copy(shipItemID, this.shipItemID, shipItemID.Length);
            Array.Copy(shipItemCount, this.shipItemCount, shipItemCount.Length);
            Array.Copy(shipPlanetA, this.shipPlanetA, shipPlanetA.Length);
            Array.Copy(shipPlanetB, this.shipPlanetB, shipPlanetB.Length);
            Array.Copy(shipOtherGId, this.shipOtherGId, shipOtherGId.Length);
            Array.Copy(shipT, this.shipT, shipT.Length);
            Array.Copy(shipIndex, this.shipIndex, shipIndex.Length);
            Array.Copy(shipPos, this.shipPos, shipPos.Length);
            Array.Copy(shipRot, this.shipRot, shipRot.Length);
            Array.Copy(shipVel, this.shipVel, shipVel.Length);
            Array.Copy(shipSpeed, this.shipSpeed, shipSpeed.Length);
            Array.Copy(shipAngularVel, this.shipAngularVel, shipAngularVel.Length);
            Array.Copy(shipPPosTemp, this.shipPPosTemp, shipPPosTemp.Length);
            Array.Copy(shipPRotTemp, this.shipPRotTemp, shipPRotTemp.Length);
        }
    }
}
