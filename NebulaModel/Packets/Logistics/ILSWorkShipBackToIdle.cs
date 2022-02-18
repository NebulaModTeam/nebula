﻿namespace NebulaModel.Packets.Logistics
{
    public class ILSWorkShipBackToIdle
    {
        public int GId { get; set; }
        public int PlanetA { get; set; }
        public int StationMaxShipCount { get; set; }
        public int ShipIndex { get; set; }
        public int WorkShipIndex { get; set; }
        public ILSWorkShipBackToIdle() { }
        public ILSWorkShipBackToIdle(StationComponent stationComponent, ShipData shipData, int workShipIndex)
        {
            GId = stationComponent.gid;
            PlanetA = shipData.planetA;
            StationMaxShipCount = stationComponent.workShipDatas.Length;
            ShipIndex = shipData.shipIndex;
            WorkShipIndex = workShipIndex;
        }
    }
}
