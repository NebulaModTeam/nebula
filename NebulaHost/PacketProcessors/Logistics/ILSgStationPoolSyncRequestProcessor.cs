using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using NebulaModel.DataStructures;

namespace NebulaHost.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    public class ILSgStationPoolSyncRequestProcessor : IPacketProcessor<ILSRequestgStationPoolSync>
    {
        private PlayerManager playerManager;
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

                int[] shipStationGId = new int[countILS * 10];
                int[] shipStage = new int[countILS * 10];
                int[] shipDirection = new int[countILS * 10];
                float[] shipWarpState = new float[countILS * 10];
                int[] shipWarperCnt = new int[countILS * 10];
                int[] shipItemID = new int[countILS * 10];
                int[] shipItemCount = new int[countILS * 10];
                int[] shipPlanetA = new int[countILS * 10];
                int[] shipPlanetB = new int[countILS * 10];
                int[] shipOtherGId = new int[countILS * 10];
                float[] shipT = new float[countILS * 10];
                int[] shipIndex = new int[countILS * 10];
                Double3[] shipPos = new Double3[countILS * 10];
                Float4[] shipRot = new Float4[countILS * 10];
                Float3[] shipVel = new Float3[countILS * 10];
                float[] shipSpeed = new float[countILS * 10];
                Float3[] shipAngularVel = new Float3[countILS * 10];
                Double3[] shipPPosTemp = new Double3[countILS * 10];
                Float4[] shipPRotTemp = new Float4[countILS * 10];

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

                        for(int j = 0; j < 10; j++)
                        {
                            ShipData shipData = stationComponent.workShipDatas[j];
                            int index = iter * 10 + j;

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
