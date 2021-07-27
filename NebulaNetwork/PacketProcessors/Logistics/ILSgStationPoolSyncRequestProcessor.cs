using NebulaModel.Attributes;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld.Logistics;

/*
 * Whenever a client connects we sync the current state of all ILS and ships to them
 * resulting in a quite large packet but its only sent one time upon client connect.
 */
namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    public class ILSgStationPoolSyncRequestProcessor : PacketProcessor<ILSRequestgStationPoolSync>
    {
        private PlayerManager playerManager;
        public ILSgStationPoolSyncRequestProcessor()
        {
            playerManager = MultiplayerHostSession.Instance?.PlayerManager;
        }
        public override void ProcessPacket(ILSRequestgStationPoolSync packet, NebulaConnection conn)
        {
            if (IsClient) return;

            Player player = playerManager.GetPlayer(conn);
            if (player == null)
            {
                player = playerManager.GetSyncingPlayer(conn);
            }
            if (player != null)
            {
                int countILS = 0;
                int iter = 0;

                foreach (StationComponent stationComponent in GameMain.data.galacticTransport.stationPool)
                {
                    if (stationComponent != null)
                    {
                        countILS++;
                    }
                }

                if (countILS == 0)
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

                int[] shipStationGId = new int[countILS * ILSShipManager.ILSMaxShipCount];
                int[] shipStage = new int[countILS * ILSShipManager.ILSMaxShipCount];
                int[] shipDirection = new int[countILS * ILSShipManager.ILSMaxShipCount];
                float[] shipWarpState = new float[countILS * ILSShipManager.ILSMaxShipCount];
                int[] shipWarperCnt = new int[countILS * ILSShipManager.ILSMaxShipCount];
                int[] shipItemID = new int[countILS * ILSShipManager.ILSMaxShipCount];
                int[] shipItemCount = new int[countILS * ILSShipManager.ILSMaxShipCount];
                int[] shipPlanetA = new int[countILS * ILSShipManager.ILSMaxShipCount];
                int[] shipPlanetB = new int[countILS * ILSShipManager.ILSMaxShipCount];
                int[] shipOtherGId = new int[countILS * ILSShipManager.ILSMaxShipCount];
                float[] shipT = new float[countILS * ILSShipManager.ILSMaxShipCount];
                int[] shipIndex = new int[countILS * ILSShipManager.ILSMaxShipCount];
                Double3[] shipPos = new Double3[countILS * ILSShipManager.ILSMaxShipCount];
                Float4[] shipRot = new Float4[countILS * ILSShipManager.ILSMaxShipCount];
                Float3[] shipVel = new Float3[countILS * ILSShipManager.ILSMaxShipCount];
                float[] shipSpeed = new float[countILS * ILSShipManager.ILSMaxShipCount];
                Float3[] shipAngularVel = new Float3[countILS * ILSShipManager.ILSMaxShipCount];
                Double3[] shipPPosTemp = new Double3[countILS * ILSShipManager.ILSMaxShipCount];
                Float4[] shipPRotTemp = new Float4[countILS * ILSShipManager.ILSMaxShipCount];

                foreach (StationComponent stationComponent in GameMain.data.galacticTransport.stationPool)
                {
                    if (stationComponent != null)
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
                        for (int j = 0; j < ILSShipManager.ILSMaxShipCount; j++)
                        {
                            ShipData shipData = stationComponent.workShipDatas[j];
                            int index = iter * ILSShipManager.ILSMaxShipCount + j;

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
