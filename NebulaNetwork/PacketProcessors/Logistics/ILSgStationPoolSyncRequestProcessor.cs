using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using System.Collections.Generic;

/*
 * Whenever a client connects we sync the current state of all ILS and ships to them
 * resulting in a quite large packet but its only sent one time upon client connect.
 */
namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    public class ILSgStationPoolSyncRequestProcessor : PacketProcessor<ILSRequestgStationPoolSync>
    {
        private readonly IPlayerManager playerManager;
        public ILSgStationPoolSyncRequestProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }
        public override void ProcessPacket(ILSRequestgStationPoolSync packet, NebulaConnection conn)
        {
            if (IsClient)
            {
                return;
            }

            INebulaPlayer player = playerManager.GetPlayer(conn);
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
                int[] stationMaxShipCount = new int[countILS];
                int[] stationId = new int[countILS];
                string[] stationName = new string[countILS];
                Float3[] DockPos = new Float3[countILS];
                Float4[] DockRot = new Float4[countILS];
                int[] planetId = new int[countILS];
                int[] workShipCount = new int[countILS];
                int[] idleShipCount = new int[countILS];
                ulong[] workShipIndices = new ulong[countILS];
                ulong[] idleShipIndices = new ulong[countILS];
                
                List<int> shipStage = new List<int>();
                List<int> shipDirection = new List<int>();
                List<float> shipWarpState = new List<float>();
                List<int> shipWarperCnt = new List<int>();
                List<int> shipItemID = new List<int>();
                List<int> shipItemCount = new List<int>();
                List<int> shipPlanetA = new List<int>();
                List<int> shipPlanetB = new List<int>();
                List<int> shipOtherGId = new List<int>();
                List<float> shipT = new List<float>();
                List<int> shipIndex = new List<int>();
                List<Double3> shipPos = new List<Double3>();
                List<Float4> shipRot = new List<Float4>();
                List<Float3> shipVel = new List<Float3>();
                List<float> shipSpeed = new List<float>();
                List<Float3> shipAngularVel = new List<Float3>();
                List<Double3> shipPPosTemp = new List<Double3>();
                List<Float4> shipPRotTemp = new List<Float4>();

                foreach (StationComponent stationComponent in GameMain.data.galacticTransport.stationPool)
                {
                    if (stationComponent != null)
                    {
                        stationGId[iter] = stationComponent.gid;
                        stationMaxShipCount[iter] = stationComponent.workShipDatas.Length;
                        stationId[iter] = stationComponent.id;
                        stationName[iter] = stationComponent.name;
                        DockPos[iter] = new Float3(stationComponent.shipDockPos);
                        DockRot[iter] = new Float4(stationComponent.shipDockRot);
                        planetId[iter] = stationComponent.planetId;
                        workShipCount[iter] = stationComponent.workShipCount;
                        idleShipCount[iter] = stationComponent.idleShipCount;
                        workShipIndices[iter] = stationComponent.workShipIndices;
                        idleShipIndices[iter] = stationComponent.idleShipIndices;

                        // ShipData is never null
                        for (int j = 0; j < stationComponent.workShipDatas.Length; j++)
                        {
                            ShipData shipData = stationComponent.workShipDatas[j];

                            shipStage.Add(shipData.stage);
                            shipDirection.Add(shipData.direction);
                            shipWarpState.Add(shipData.warpState);
                            shipWarperCnt.Add(shipData.warperCnt);
                            shipItemID.Add(shipData.itemId);
                            shipItemCount.Add(shipData.itemCount);
                            shipPlanetA.Add(shipData.planetA);
                            shipPlanetB.Add(shipData.planetB);
                            shipOtherGId.Add(shipData.otherGId);
                            shipT.Add(shipData.t);
                            shipIndex.Add(shipData.shipIndex);
                            shipPos.Add(new Double3(shipData.uPos.x, shipData.uPos.y, shipData.uPos.z));
                            shipRot.Add(new Float4(shipData.uRot));
                            shipVel.Add(new Float3(shipData.uVel));
                            shipSpeed.Add(shipData.uSpeed);
                            shipAngularVel.Add(new Float3(shipData.uAngularVel));
                            shipPPosTemp.Add(new Double3(shipData.pPosTemp.x, shipData.pPosTemp.y, shipData.pPosTemp.z));
                            shipPRotTemp.Add(new Float4(shipData.pRotTemp));
                        }

                        iter++;
                    }
                }

                ILSgStationPoolSync packet2 = new ILSgStationPoolSync(
                    stationGId,
                    stationMaxShipCount,
                    stationId,
                    stationName,
                    DockPos,
                    DockRot,
                    planetId,
                    workShipCount,
                    idleShipCount,
                    workShipIndices,
                    idleShipIndices,
                    shipStage.ToArray(),
                    shipDirection.ToArray(),
                    shipWarpState.ToArray(),
                    shipWarperCnt.ToArray(),
                    shipItemID.ToArray(),
                    shipItemCount.ToArray(),
                    shipPlanetA.ToArray(),
                    shipPlanetB.ToArray(),
                    shipOtherGId.ToArray(),
                    shipT.ToArray(),
                    shipIndex.ToArray(),
                    shipPos.ToArray(),
                    shipRot.ToArray(),
                    shipVel.ToArray(),
                    shipSpeed.ToArray(),
                    shipAngularVel.ToArray(),
                    shipPPosTemp.ToArray(),
                    shipPRotTemp.ToArray());
                player.SendPacket(packet2);
            }
        }
    }
}
