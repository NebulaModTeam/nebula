using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using NebulaModel.DataStructures;

/*
 * Whenever a client connects we sync the current state of all ILS and ships to them
 * resulting in a quite large packet but its only sent one time upon client connect.
 */
namespace NebulaHost.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    public class ILSgStationPoolSyncRequestProcessor : IPacketProcessor<ILSRequestgStationPoolSync>
    {
        private PlayerManager playerManager;
        private const int maxShipsPerILS = 10;
        public ILSgStationPoolSyncRequestProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }
        public void ProcessPacket(ILSRequestgStationPoolSync packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if(player == null)
            {
                player = playerManager.GetSyncingPlayer(conn);
            }
            if (player != null)
            {
                int countILS = 0;
                int iter = 0;

                foreach(StationComponent stationComponent in GameMain.data.galacticTransport.stationPool)
                {
                    if(stationComponent != null)
                    {
                        countILS++;
                    }
                }

                if(countILS == 0)
                {
                    return;
                }

                int[] stationGId = new int[countILS];
                int[] stationId = new int[countILS];
                Float3[] DockPos = new Float3[countILS];
                Float4[] DockRot = new Float4[countILS];
                int[] planetId = new int[countILS];
                int[] workShipCount = new int[countILS];
                int[] idleShipCount = new int[countILS];
                ulong[] workShipIndices = new ulong[countILS];
                ulong[] idleShipIndices = new ulong[countILS];

                int[] shipStationGId = new int[countILS * maxShipsPerILS];
                int[] shipStage = new int[countILS * maxShipsPerILS];
                int[] shipDirection = new int[countILS * maxShipsPerILS];
                float[] shipWarpState = new float[countILS * maxShipsPerILS];
                int[] shipWarperCnt = new int[countILS * maxShipsPerILS];
                int[] shipItemID = new int[countILS * maxShipsPerILS];
                int[] shipItemCount = new int[countILS * maxShipsPerILS];
                int[] shipPlanetA = new int[countILS * maxShipsPerILS];
                int[] shipPlanetB = new int[countILS * maxShipsPerILS];
                int[] shipOtherGId = new int[countILS * maxShipsPerILS];
                float[] shipT = new float[countILS * maxShipsPerILS];
                int[] shipIndex = new int[countILS * maxShipsPerILS];
                Double3[] shipPos = new Double3[countILS * maxShipsPerILS];
                Float4[] shipRot = new Float4[countILS * maxShipsPerILS];
                Float3[] shipVel = new Float3[countILS * maxShipsPerILS];
                float[] shipSpeed = new float[countILS * maxShipsPerILS];
                Float3[] shipAngularVel = new Float3[countILS * maxShipsPerILS];
                Double3[] shipPPosTemp = new Double3[countILS * maxShipsPerILS];
                Float4[] shipPRotTemp = new Float4[countILS * maxShipsPerILS];

                foreach(StationComponent stationComponent in GameMain.data.galacticTransport.stationPool)
                {
                    if(stationComponent != null)
                    {
                        stationGId[iter] = stationComponent.gid;
                        stationId[iter] = stationComponent.id;
                        DockPos[iter] = new Float3(stationComponent.shipDockPos);
                        DockRot[iter] = new Float4(stationComponent.shipDockRot);
                        planetId[iter] = stationComponent.planetId;
                        workShipCount[iter] = stationComponent.workShipCount;
                        idleShipCount[iter] = stationComponent.idleShipCount;
                        workShipIndices[iter] = stationComponent.workShipIndices;
                        idleShipIndices[iter] = stationComponent.idleShipIndices;

                        // ShipData is never null
                        for(int j = 0; j < maxShipsPerILS; j++)
                        {
                            ShipData shipData = stationComponent.workShipDatas[j];
                            int index = iter * maxShipsPerILS + j;

                            shipStationGId[index] = stationComponent.gid;
                            shipStage[index] = shipData.stage;
                            shipDirection[index] = shipData.direction;
                            shipWarpState[index] = shipData.warpState;
                            shipWarperCnt[index] = shipData.warperCnt;
                            shipItemID[index] = shipData.itemId;
                            shipItemCount[index] = shipData.itemCount;
                            shipPlanetA[index] = shipData.planetA;
                            shipPlanetB[index] = shipData.planetB;
                            shipOtherGId[index] = shipData.otherGId;
                            shipT[index] = shipData.t;
                            shipIndex[index] = shipData.shipIndex;
                            shipPos[index] = new Double3(shipData.uPos.x, shipData.uPos.y, shipData.uPos.z);
                            shipRot[index] = new Float4(shipData.uRot);
                            shipVel[index] = new Float3(shipData.uVel);
                            shipSpeed[index] = shipData.uSpeed;
                            shipAngularVel[index] = new Float3(shipData.uAngularVel);
                            shipPPosTemp[index] = new Double3(shipData.pPosTemp.x, shipData.pPosTemp.y, shipData.pPosTemp.z);
                            shipPRotTemp[index] = new Float4(shipData.pRotTemp);
                        }

                        iter++;
                    }
                }

                ILSgStationPoolSync packet2 = new ILSgStationPoolSync(
                    stationGId,
                    stationId,
                    DockPos,
                    DockRot,
                    planetId,
                    workShipCount,
                    idleShipCount,
                    workShipIndices,
                    idleShipIndices,
                    shipStationGId,
                    shipStage,
                    shipDirection,
                    shipWarpState,
                    shipWarperCnt,
                    shipItemID,
                    shipItemCount,
                    shipPlanetA,
                    shipPlanetB,
                    shipOtherGId,
                    shipT,
                    shipIndex,
                    shipPos,
                    shipRot,
                    shipVel,
                    shipSpeed,
                    shipAngularVel,
                    shipPPosTemp,
                    shipPRotTemp);
                player.SendPacket(packet2);
            }
        }
    }
}
