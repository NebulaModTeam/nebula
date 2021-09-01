using NebulaAPI;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
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
                Multiplayer.Session.Ships.CreateFakeStationComponent(packet.stationGId[i], packet.planetId[i], false); // handles array resizing
                gStationPool = GameMain.data.galacticTransport.stationPool; // dont remove or you get an ArrayOutOfBounds

                gStationPool[packet.stationGId[i]].shipDockPos = packet.DockPos[i].ToVector3();

                gStationPool[packet.stationGId[i]].shipDockRot = packet.DockRot[i].ToQuaternion();

                gStationPool[packet.stationGId[i]].id = packet.stationId[i];
                gStationPool[packet.stationGId[i]].planetId = packet.planetId[i];
                gStationPool[packet.stationGId[i]].workShipCount = packet.workShipCount[i];
                gStationPool[packet.stationGId[i]].idleShipCount = packet.idleShipCount[i];
                gStationPool[packet.stationGId[i]].workShipIndices = packet.workShipIndices[i];
                gStationPool[packet.stationGId[i]].idleShipIndices = packet.idleShipIndices[i];
                gStationPool[packet.stationGId[i]].shipRenderers = new ShipRenderingData[Multiplayer.Session.Ships.ILSMaxShipCount];
                gStationPool[packet.stationGId[i]].shipUIRenderers = new ShipUIRenderingData[Multiplayer.Session.Ships.ILSMaxShipCount];

                gStationPool[packet.stationGId[i]].shipDiskPos = new Vector3[Multiplayer.Session.Ships.ILSMaxShipCount];
                gStationPool[packet.stationGId[i]].shipDiskRot = new Quaternion[Multiplayer.Session.Ships.ILSMaxShipCount];

                // theese are the individual landing places for the ships on the station's disk at the top
                for (int j = 0; j < Multiplayer.Session.Ships.ILSMaxShipCount; j++)
                {
                    gStationPool[packet.stationGId[i]].shipDiskRot[j] = Quaternion.Euler(0f, 360f / (float)Multiplayer.Session.Ships.ILSMaxShipCount * (float)j, 0f);
                    gStationPool[packet.stationGId[i]].shipDiskPos[j] = gStationPool[packet.stationGId[i]].shipDiskRot[j] * new Vector3(0f, 0f, 11.5f);
                }
                for (int j = 0; j < Multiplayer.Session.Ships.ILSMaxShipCount; j++)
                {
                    gStationPool[packet.stationGId[i]].shipDiskRot[j] = gStationPool[packet.stationGId[i]].shipDockRot * gStationPool[packet.stationGId[i]].shipDiskRot[j];
                    gStationPool[packet.stationGId[i]].shipDiskPos[j] = gStationPool[packet.stationGId[i]].shipDockPos + gStationPool[packet.stationGId[i]].shipDockRot * gStationPool[packet.stationGId[i]].shipDiskPos[j];
                }
            }

            // thanks Baldy for the fix :D
            // nearly lost all my hairs because of it
            for (int i = 0; i < packet.shipStationGId.Length; i++)
            {
                ShipData shipData = gStationPool[packet.shipStationGId[i]].workShipDatas[i % Multiplayer.Session.Ships.ILSMaxShipCount];
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

                shipData.uPos = packet.shipPos[i].ToVectorLF3();
                shipData.uRot = packet.shipRot[i].ToQuaternion();
                shipData.uVel = packet.shipVel[i].ToVector3();
                shipData.uSpeed = packet.shipSpeed[i];
                shipData.uAngularVel = packet.shipAngularVel[i].ToVector3();
                shipData.pPosTemp = packet.shipPPosTemp[i].ToVectorLF3();
                shipData.pRotTemp = packet.shipRot[i].ToQuaternion();

                gStationPool[packet.shipStationGId[i]].workShipDatas[i % Multiplayer.Session.Ships.ILSMaxShipCount] = shipData;
            }

            gTransport.Arragement();
        }
    }
}
