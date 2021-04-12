using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using NebulaWorld.Logistics;

namespace NebulaClient.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class ILSgStationPoolSyncProcessor : IPacketProcessor<ILSgStationPoolSync>
    {
        public void ProcessPacket(ILSgStationPoolSync packet, NebulaConnection conn)
        {
            GalacticTransport gTransport = GameMain.data.galacticTransport;
            StationComponent[] gStationPool = GameMain.data.galacticTransport.stationPool;

            for (int i = 0; i < packet.stationGId.Length; i++)
            {
                ILSShipManager.CreateFakeStationComponent(packet.stationGId[i], packet.planetId[i]); // handles array resizing
                gStationPool = GameMain.data.galacticTransport.stationPool; // dont remove or you get an ArrayOutOfBounds

                gStationPool[packet.stationGId[i]].shipDockPos.x = packet.DockPos[i].x;
                gStationPool[packet.stationGId[i]].shipDockPos.y = packet.DockPos[i].y;
                gStationPool[packet.stationGId[i]].shipDockPos.z = packet.DockPos[i].z;
                
                gStationPool[packet.stationGId[i]].shipDockRot.x = packet.DockRot[i].x;
                gStationPool[packet.stationGId[i]].shipDockRot.y = packet.DockRot[i].y;
                gStationPool[packet.stationGId[i]].shipDockRot.z = packet.DockRot[i].z;
                gStationPool[packet.stationGId[i]].shipDockRot.w = packet.DockRot[i].w;
                
                gStationPool[packet.stationGId[i]].planetId = packet.planetId[i];
                gStationPool[packet.stationGId[i]].workShipCount = packet.workShipCount[i];
                gStationPool[packet.stationGId[i]].idleShipCount = packet.idleShipCount[i];
            }

            for(int i = 0; i < packet.shipStationGId.Length; i++)
            {
                ShipData shipData = gStationPool[packet.shipStationGId[i]].workShipDatas[packet.shipIndex[i]];
                shipData.stage = packet.shipStage[i];
                shipData.direction = packet.shipDirection[i];
                shipData.itemId = packet.shipItemID[i];
                shipData.itemCount = packet.shipItemCount[i];
                shipData.planetA = packet.shipPlanetA[i];
                shipData.planetB = packet.shipPlanetB[i];
                shipData.shipIndex = packet.shipIndex[i];

                shipData.uPos.x = packet.shipPos[i].x;
                shipData.uPos.y = packet.shipPos[i].y;
                shipData.uPos.z = packet.shipPos[i].z;

                shipData.uRot.x = packet.shipRot[i].x;
                shipData.uRot.y = packet.shipRot[i].y;
                shipData.uRot.z = packet.shipRot[i].z;
                shipData.uRot.w = packet.shipRot[i].w;

                shipData.uVel.x = packet.shipVel[i].x;
                shipData.uVel.y = packet.shipVel[i].y;
                shipData.uVel.z = packet.shipVel[i].z;

                shipData.uSpeed = packet.shipSpeed[i];
            }

            gTransport.Arragement();
        }
    }
}
