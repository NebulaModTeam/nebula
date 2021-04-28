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
            this.stationGId = (int[])stationGId.Clone();
            this.stationId = (int[])stationId.Clone();
            this.DockPos = (Float3[])DockPos.Clone();
            this.DockRot = (Float4[])DockRot.Clone();
            this.planetId = (int[])planetId.Clone();
            this.workShipCount = (int[])workShipCount.Clone();
            this.idleShipCount = (int[])idleShipCount.Clone();
            this.workShipIndices = (ulong[])workShipIndices.Clone();
            this.idleShipIndices = (ulong[])idleShipIndices.Clone();
            this.shipStationGId = (int[])shipStationGId.Clone();
            this.shipStage = (int[])shipStage.Clone();
            this.shipDirection = (int[])shipDirection.Clone();
            this.shipWarpState = (float[])shipWarpState.Clone();
            this.shipWarperCnt = (int[])shipWarperCnt.Clone();
            this.shipItemID = (int[])shipItemID.Clone();
            this.shipItemCount = (int[])shipItemCount.Clone();
            this.shipPlanetA = (int[])shipPlanetA.Clone();
            this.shipPlanetB = (int[])shipPlanetB.Clone();
            this.shipOtherGId = (int[])shipOtherGId.Clone();
            this.shipT = (float[])shipT.Clone();
            this.shipIndex = (int[])shipIndex.Clone();
            this.shipPos = (Double3[])shipPos.Clone();
            this.shipRot = (Float4[])shipRot.Clone();
            this.shipVel = (Float3[])shipVel.Clone();
            this.shipSpeed = (float[])shipSpeed.Clone();
            this.shipAngularVel = (Float3[])shipAngularVel.Clone();
            this.shipPPosTemp = (Double3[])shipPPosTemp.Clone();
            this.shipPRotTemp = (Float4[])shipPRotTemp.Clone();
        }
    }
}
