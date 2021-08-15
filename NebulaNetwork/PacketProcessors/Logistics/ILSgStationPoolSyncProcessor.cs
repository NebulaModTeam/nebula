using NebulaModel.Attributes;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld.Logistics;
using UnityEngine;

/*
 * This packet is only sent one time when a client joins the game
 * it is used to sync the gStationPool to the clients including all ships.
 * This is needed to have the current state and position of ships
 */
namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class ILSgStationPoolSyncProcessor : PacketProcessor<ILSgStationPoolSync>
    {
        public override void ProcessPacket(ILSgStationPoolSync packet, NebulaConnection conn)
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
                gStationPool[packet.stationGId[i]].shipRenderers = new ShipRenderingData[ILSShipManager.ILSMaxShipCount];
                gStationPool[packet.stationGId[i]].shipUIRenderers = new ShipUIRenderingData[ILSShipManager.ILSMaxShipCount];

                gStationPool[packet.stationGId[i]].shipDiskPos = new Vector3[ILSShipManager.ILSMaxShipCount];
                gStationPool[packet.stationGId[i]].shipDiskRot = new Quaternion[ILSShipManager.ILSMaxShipCount];

                // theese are the individual landing places for the ships on the station's disk at the top
                for (int j = 0; j < ILSShipManager.ILSMaxShipCount; j++)
                {
                    gStationPool[packet.stationGId[i]].shipDiskRot[j] = Quaternion.Euler(0f, 360f / (float)ILSShipManager.ILSMaxShipCount * (float)j, 0f);
                    gStationPool[packet.stationGId[i]].shipDiskPos[j] = gStationPool[packet.stationGId[i]].shipDiskRot[j] * new Vector3(0f, 0f, 11.5f);
                }
                for (int j = 0; j < ILSShipManager.ILSMaxShipCount; j++)
                {
                    gStationPool[packet.stationGId[i]].shipDiskRot[j] = gStationPool[packet.stationGId[i]].shipDockRot * gStationPool[packet.stationGId[i]].shipDiskRot[j];
                    gStationPool[packet.stationGId[i]].shipDiskPos[j] = gStationPool[packet.stationGId[i]].shipDockPos + gStationPool[packet.stationGId[i]].shipDockRot * gStationPool[packet.stationGId[i]].shipDiskPos[j];
                }
            }

            // thanks Baldy for the fix :D
            // nearly lost all my hairs because of it
            for (int i = 0; i < packet.shipStationGId.Length; i++)
            {
                ShipData shipData = gStationPool[packet.shipStationGId[i]].workShipDatas[i % ILSShipManager.ILSMaxShipCount];
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

                gStationPool[packet.shipStationGId[i]].workShipDatas[i % ILSShipManager.ILSMaxShipCount] = shipData;
            }

            gTransport.Arragement();
        }
    }
}
