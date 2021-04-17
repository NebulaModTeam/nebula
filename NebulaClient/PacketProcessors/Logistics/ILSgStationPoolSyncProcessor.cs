using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using NebulaModel.DataStructures;
using NebulaWorld.Logistics;
using UnityEngine;

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
                ILSShipManager.CreateFakeStationComponent(packet.stationGId[i], packet.planetId[i], false); // handles array resizing
                gStationPool = GameMain.data.galacticTransport.stationPool; // dont remove or you get an ArrayOutOfBounds

                gStationPool[packet.stationGId[i]].shipDockPos = DataStructureExtensions.ToVector3(packet.DockPos[i]);

                gStationPool[packet.stationGId[i]].shipDockRot = DataStructureExtensions.ToQuaternion(packet.DockRot[i]);

                gStationPool[packet.stationGId[i]].id = packet.stationId[i];
                gStationPool[packet.stationGId[i]].planetId = packet.planetId[i];
                gStationPool[packet.stationGId[i]].workShipCount = packet.workShipCount[i];
                gStationPool[packet.stationGId[i]].idleShipCount = packet.idleShipCount[i];
                gStationPool[packet.stationGId[i]].workShipIndices = packet.workShipIndices[i];
                gStationPool[packet.stationGId[i]].idleShipIndices = packet.idleShipIndices[i];

                gStationPool[packet.stationGId[i]].shipDiskPos = new Vector3[10];
                gStationPool[packet.stationGId[i]].shipDiskRot = new Quaternion[10];

                for (int j = 0; j < 10; j++)
                {
                    gStationPool[packet.stationGId[i]].shipDiskRot[j] = Quaternion.Euler(0f, 360f / (float)10 * (float)j, 0f);
                    gStationPool[packet.stationGId[i]].shipDiskPos[j] = gStationPool[packet.stationGId[i]].shipDiskRot[j] * new Vector3(0f, 0f, 11.5f);
                }
                for (int j = 0; j < 10; j++)
                {
                    gStationPool[packet.stationGId[i]].shipDiskRot[j] = gStationPool[packet.stationGId[i]].shipDockRot * gStationPool[packet.stationGId[i]].shipDiskRot[j];
                    gStationPool[packet.stationGId[i]].shipDiskPos[j] = gStationPool[packet.stationGId[i]].shipDockPos + gStationPool[packet.stationGId[i]].shipDockRot * gStationPool[packet.stationGId[i]].shipDiskPos[j];
                }
            }

            // thanks Baldy for the fix :D
            // nearly lost all my hairs because of it
            for(int i = 0; i < packet.shipStationGId.Length; i++)
            {
                ShipData shipData = gStationPool[packet.shipStationGId[i]].workShipDatas[i % 10];
                shipData.stage = packet.shipStage[i];
                shipData.direction = packet.shipDirection[i];
                shipData.warpState = packet.shipWarpState[i];
                shipData.warperCnt = packet.shipWarperCnt[i];
                shipData.itemId = packet.shipItemID[i];
                shipData.itemCount = packet.shipItemCount[i];
                shipData.planetA = packet.shipPlanetA[i];
                shipData.planetB = packet.shipPlanetB[i];
                shipData.otherGId = packet.shipOtherGId[i];
                shipData.t = packet.shipT[i];
                shipData.shipIndex = packet.shipIndex[i];

                shipData.uPos = DataStructureExtensions.ToVectorLF3(packet.shipPos[i]);
                shipData.uRot = DataStructureExtensions.ToQuaternion(packet.shipRot[i]);
                shipData.uVel = DataStructureExtensions.ToVector3(packet.shipVel[i]);
                shipData.uSpeed = packet.shipSpeed[i];
                shipData.uAngularVel = DataStructureExtensions.ToVector3(packet.shipAngularVel[i]);
                shipData.pPosTemp = DataStructureExtensions.ToVectorLF3(packet.shipPPosTemp[i]);
                shipData.pRotTemp = DataStructureExtensions.ToQuaternion(packet.shipPRotTemp[i]);

                gStationPool[packet.shipStationGId[i]].workShipDatas[i % 10] = shipData;
            }

            gTransport.Arragement();
            Debug.Log("CURSOR NOW AT: " + GameMain.data.galacticTransport.stationCursor);
        }
    }
}
