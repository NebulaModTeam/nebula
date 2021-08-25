using NebulaModel;
using NebulaModel.Attributes;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
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
        private IPlayerManager playerManager;
        public ILSgStationPoolSyncRequestProcessor()
        {
            playerManager = Multiplayer.Session?.Network.PlayerManager;
        }
        public override void ProcessPacket(ILSRequestgStationPoolSync packet, NebulaConnection conn)
        {
            if (IsClient) return;

            NebulaPlayer player = playerManager.GetPlayer(conn);
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

                int[] shipStationGId = new int[countILS * Multiplayer.Session.Ships.ILSMaxShipCount];
                int[] shipStage = new int[countILS * Multiplayer.Session.Ships.ILSMaxShipCount];
                int[] shipDirection = new int[countILS * Multiplayer.Session.Ships.ILSMaxShipCount];
                float[] shipWarpState = new float[countILS * Multiplayer.Session.Ships.ILSMaxShipCount];
                int[] shipWarperCnt = new int[countILS * Multiplayer.Session.Ships.ILSMaxShipCount];
                int[] shipItemID = new int[countILS * Multiplayer.Session.Ships.ILSMaxShipCount];
                int[] shipItemCount = new int[countILS * Multiplayer.Session.Ships.ILSMaxShipCount];
                int[] shipPlanetA = new int[countILS * Multiplayer.Session.Ships.ILSMaxShipCount];
                int[] shipPlanetB = new int[countILS * Multiplayer.Session.Ships.ILSMaxShipCount];
                int[] shipOtherGId = new int[countILS * Multiplayer.Session.Ships.ILSMaxShipCount];
                float[] shipT = new float[countILS * Multiplayer.Session.Ships.ILSMaxShipCount];
                int[] shipIndex = new int[countILS * Multiplayer.Session.Ships.ILSMaxShipCount];
                Double3[] shipPos = new Double3[countILS * Multiplayer.Session.Ships.ILSMaxShipCount];
                Float4[] shipRot = new Float4[countILS * Multiplayer.Session.Ships.ILSMaxShipCount];
                Float3[] shipVel = new Float3[countILS * Multiplayer.Session.Ships.ILSMaxShipCount];
                float[] shipSpeed = new float[countILS * Multiplayer.Session.Ships.ILSMaxShipCount];
                Float3[] shipAngularVel = new Float3[countILS * Multiplayer.Session.Ships.ILSMaxShipCount];
                Double3[] shipPPosTemp = new Double3[countILS * Multiplayer.Session.Ships.ILSMaxShipCount];
                Float4[] shipPRotTemp = new Float4[countILS * Multiplayer.Session.Ships.ILSMaxShipCount];

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
                        for (int j = 0; j < Multiplayer.Session.Ships.ILSMaxShipCount; j++)
                        {
                            ShipData shipData = stationComponent.workShipDatas[j];
                            int index = iter * Multiplayer.Session.Ships.ILSMaxShipCount + j;

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
