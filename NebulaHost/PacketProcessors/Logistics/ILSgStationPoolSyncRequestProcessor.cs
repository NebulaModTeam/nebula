using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using NebulaModel.DataStructures;
using System.Collections.Generic;

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
                List<int> stationGId = new List<int>();
                List<int> stationId = new List<int>();
                List<Float3> DockPos = new List<Float3>();
                List<Float4> DockRot = new List<Float4>();
                List<int> planetId = new List<int>();
                List<int> workShipCount = new List<int>();
                List<int> idleShipCount = new List<int>();
                List<ulong> workShipIndices = new List<ulong>();
                List<ulong> idleShipIndices = new List<ulong>();
                List<int> shipStationGId = new List<int>();
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

                foreach(StationComponent stationComponent in GameMain.data.galacticTransport.stationPool)
                {
                    if(stationComponent != null)
                    {
                        stationGId.Add(stationComponent.gid);
                        stationId.Add(stationComponent.id);
                        DockPos.Add(new Float3(stationComponent.shipDockPos));
                        DockRot.Add(new Float4(stationComponent.shipDockRot));
                        planetId.Add(stationComponent.planetId);
                        workShipCount.Add(stationComponent.workShipCount);
                        idleShipCount.Add(stationComponent.idleShipCount);
                        workShipIndices.Add(stationComponent.workShipIndices);
                        idleShipIndices.Add(stationComponent.idleShipIndices);

                        foreach(ShipData shipData in stationComponent.workShipDatas)
                        {
                            shipStationGId.Add(stationComponent.gid);
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
                    }
                }

                ILSgStationPoolSync packet2 = new ILSgStationPoolSync(
                    stationGId.ToArray(),
                    stationId.ToArray(),
                    DockPos.ToArray(),
                    DockRot.ToArray(),
                    planetId.ToArray(),
                    workShipCount.ToArray(),
                    idleShipCount.ToArray(),
                    workShipIndices.ToArray(),
                    idleShipIndices.ToArray(),
                    shipStationGId.ToArray(),
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
