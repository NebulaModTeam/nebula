using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using NebulaModel.DataStructures;
using System.Collections.Generic;

namespace NebulaHost.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    public class ILSgStationPoolSyncRequestProcessor : IPacketProcessor<ILSgStationPoolSyncRequest>
    {
        private PlayerManager playerManager;
        public ILSgStationPoolSyncRequestProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }
        public void ProcessPacket(ILSgStationPoolSyncRequest packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if(player == null)
            {
                player = playerManager.GetSyncingPlayer(conn);
            }
            if (player != null)
            {
                List<int> stationGId = new List<int>();
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
                List<int> shipItemID = new List<int>();
                List<int> shipItemCount = new List<int>();
                List<int> shipPlanetA = new List<int>();
                List<int> shipPlanetB = new List<int>();
                List<int> shipIndex = new List<int>();
                List<Float3> shipPos = new List<Float3>();
                List<Float4> shipRot = new List<Float4>();
                List<Float3> shipVel = new List<Float3>();
                List<float> shipSpeed = new List<float>();
                List<Float3> shipAngularVel = new List<Float3>();
                List<Float3> shipPPosTemp = new List<Float3>();
                List<Float4> shipPRotTemp = new List<Float4>();

                foreach(StationComponent stationComponent in GameMain.data.galacticTransport.stationPool)
                {
                    if(stationComponent != null)
                    {
                        stationGId.Add(stationComponent.gid);
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
                            shipItemID.Add(shipData.itemId);
                            shipItemCount.Add(shipData.itemCount);
                            shipPlanetA.Add(shipData.planetA);
                            shipPlanetB.Add(shipData.planetB);
                            shipIndex.Add(shipData.shipIndex);
                            shipPos.Add(new Float3(shipData.uPos));
                            shipRot.Add(new Float4(shipData.uRot));
                            shipVel.Add(new Float3(shipData.uVel));
                            shipSpeed.Add(shipData.uSpeed);
                            shipAngularVel.Add(new Float3(shipData.uAngularVel));
                            shipPPosTemp.Add(new Float3(shipData.pPosTemp));
                            shipPRotTemp.Add(new Float4(shipData.pRotTemp));
                        }
                    }
                }

                ILSgStationPoolSync packet2 = new ILSgStationPoolSync(
                    stationGId.ToArray(),
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
                    shipItemID.ToArray(),
                    shipItemCount.ToArray(),
                    shipPlanetA.ToArray(),
                    shipPlanetB.ToArray(),
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
