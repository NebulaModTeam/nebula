using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using UnityEngine;

/*
 * This packet is only sent one time when a client joins the game
 * it is used to sync the gStationPool to the clients including all ships.
 * This is needed to have the current state and position of ships
 */
namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    internal class ILSgStationPoolSyncProcessor : PacketProcessor<ILSgStationPoolSync>
    {
        public override void ProcessPacket(ILSgStationPoolSync packet, NebulaConnection conn)
        {
            GalacticTransport gTransport = GameMain.data.galacticTransport;
            StationComponent[] gStationPool = GameMain.data.galacticTransport.stationPool;

            int arrayStartPos = 0;

            for (int i = 0; i < packet.stationGId.Length; i++)
            {
                Multiplayer.Session.Ships.CreateFakeStationComponent(packet.stationGId[i], packet.planetId[i], packet.stationMaxShipCount[i], false); // handles array resizing
                gStationPool = GameMain.data.galacticTransport.stationPool; // dont remove or you get an ArrayOutOfBounds

                gStationPool[packet.stationGId[i]].shipDockPos = packet.DockPos[i].ToVector3();

                gStationPool[packet.stationGId[i]].shipDockRot = packet.DockRot[i].ToQuaternion();

                gStationPool[packet.stationGId[i]].id = packet.stationId[i];
                gStationPool[packet.stationGId[i]].name = string.IsNullOrEmpty(packet.stationName[i]) ? null : packet.stationName[i];
                gStationPool[packet.stationGId[i]].planetId = packet.planetId[i];
                gStationPool[packet.stationGId[i]].workShipCount = packet.workShipCount[i];
                gStationPool[packet.stationGId[i]].idleShipCount = packet.idleShipCount[i];
                gStationPool[packet.stationGId[i]].workShipIndices = packet.workShipIndices[i];
                gStationPool[packet.stationGId[i]].idleShipIndices = packet.idleShipIndices[i];
                gStationPool[packet.stationGId[i]].shipRenderers = new ShipRenderingData[packet.stationMaxShipCount[i]];
                gStationPool[packet.stationGId[i]].shipUIRenderers = new ShipUIRenderingData[packet.stationMaxShipCount[i]];

                gStationPool[packet.stationGId[i]].shipDiskPos = new Vector3[packet.stationMaxShipCount[i]];
                gStationPool[packet.stationGId[i]].shipDiskRot = new Quaternion[packet.stationMaxShipCount[i]];

                // these are the individual landing places for the ships on the station's disk at the top
                for (int j = 0; j < packet.stationMaxShipCount[i]; j++)
                {
                    gStationPool[packet.stationGId[i]].shipDiskRot[j] = Quaternion.Euler(0f, 360f / packet.stationMaxShipCount[i] * j, 0f);
                    gStationPool[packet.stationGId[i]].shipDiskPos[j] = gStationPool[packet.stationGId[i]].shipDiskRot[j] * new Vector3(0f, 0f, 11.5f);
                }
                for (int j = 0; j < packet.stationMaxShipCount[i]; j++)
                {
                    gStationPool[packet.stationGId[i]].shipDiskRot[j] = gStationPool[packet.stationGId[i]].shipDockRot * gStationPool[packet.stationGId[i]].shipDiskRot[j];
                    gStationPool[packet.stationGId[i]].shipDiskPos[j] = gStationPool[packet.stationGId[i]].shipDockPos + gStationPool[packet.stationGId[i]].shipDockRot * gStationPool[packet.stationGId[i]].shipDiskPos[j];
                }
                
                for (int j = 0; j < packet.stationMaxShipCount[i]; j++)
                {
                    int index = arrayStartPos + j;
                    ShipData shipData = gStationPool[packet.stationGId[i]].workShipDatas[j];
                    shipData.stage = packet.shipStage[index];
                    shipData.direction = packet.shipDirection[index];
                    shipData.warpState = packet.shipWarpState[index];
                    shipData.warperCnt = packet.shipWarperCnt[index];
                    shipData.itemId = packet.shipItemID[index];
                    shipData.itemCount = packet.shipItemCount[index];
                    shipData.planetA = packet.shipPlanetA[index];
                    shipData.planetB = packet.shipPlanetB[index];
                    shipData.otherGId = packet.shipOtherGId[index];
                    shipData.t = packet.shipT[index];
                    shipData.shipIndex = packet.shipIndex[index];

                    shipData.uPos = packet.shipPos[index].ToVectorLF3();
                    shipData.uRot = packet.shipRot[index].ToQuaternion();
                    shipData.uVel = packet.shipVel[index].ToVector3();
                    shipData.uSpeed = packet.shipSpeed[index];
                    shipData.uAngularVel = packet.shipAngularVel[index].ToVector3();
                    shipData.pPosTemp = packet.shipPPosTemp[index].ToVectorLF3();
                    shipData.pRotTemp = packet.shipRot[index].ToQuaternion();

                    gStationPool[packet.stationGId[i]].workShipDatas[j] = shipData;
                }

                arrayStartPos += packet.stationMaxShipCount[i];
            }

            gTransport.Arragement();
        }
    }
}
