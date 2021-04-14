using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using NebulaModel.DataStructures;
using System.Collections.Generic;

namespace NebulaHost.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    public class ILSRequestShipDockProcessor: IPacketProcessor<ILSRequestShipDock>
    {
        private PlayerManager playerManager;
        public ILSRequestShipDockProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }
        public void ProcessPacket(ILSRequestShipDock packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if(player == null)
            {
                player = playerManager.GetSyncingPlayer(conn);
            }
            if (player != null && GameMain.data.galacticTransport.stationCapacity > packet.stationGId)
            {
                List<int> shipOtherGId = new List<int>();
                List<int> shipIndex = new List<int>();
                List<Double3> shipPos = new List<Double3>();
                List<Float4> shipRot = new List<Float4>();
                List<Double3> shipPPosTemp = new List<Double3>();
                List<Float4> shipPRotTemp = new List<Float4>();

                // this is a very slow way to find ShipData that has otherGId set to packet.stationGId
                // TODO: find a faster way
                for(int i = 0; i < GameMain.data.galacticTransport.stationCapacity; i++)
                {
                    if (GameMain.data.galacticTransport.stationPool[i] != null)
                    {
                        ShipData[] shipData = GameMain.data.galacticTransport.stationPool[i].workShipDatas;

                        for (int j = 0; j < GameMain.data.galacticTransport.stationPool[i].workShipDatas.Length; j++)
                        {
                            if (shipData[j].otherGId == packet.stationGId)
                            {
                                shipOtherGId.Add(shipData[i].otherGId);
                                shipIndex.Add(shipData[j].shipIndex);
                                shipPos.Add(new Double3(shipData[j].uPos.x, shipData[j].uPos.y, shipData[j].uPos.z));
                                shipRot.Add(new Float4(shipData[j].uRot));
                                shipPPosTemp.Add(new Double3(shipData[j].pPosTemp.x, shipData[j].pPosTemp.y, shipData[j].pPosTemp.z));
                                shipPRotTemp.Add(new Float4(shipData[j].pRotTemp));
                            }
                        }
                    }
                }

                ILSRequestShipDock packet2 = new ILSRequestShipDock(packet.stationGId,
                                                                    GameMain.data.galacticTransport.stationPool[packet.stationGId].shipDockPos,
                                                                    GameMain.data.galacticTransport.stationPool[packet.stationGId].shipDockRot,
                                                                    shipOtherGId.ToArray(),
                                                                    shipIndex.ToArray(),
                                                                    shipPos.ToArray(),
                                                                    shipRot.ToArray(),
                                                                    shipPPosTemp.ToArray(),
                                                                    shipPRotTemp.ToArray());
                player.SendPacket(packet2);
            }
        }
    }
}
