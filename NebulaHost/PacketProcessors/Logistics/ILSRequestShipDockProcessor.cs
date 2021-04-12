using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using UnityEngine;

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
                ILSRequestShipDock packet2 = new ILSRequestShipDock(packet.stationGId, GameMain.data.galacticTransport.stationPool[packet.stationGId].shipDockPos, GameMain.data.galacticTransport.stationPool[packet.stationGId].shipDockRot);
                player.SendPacket(packet2);
            }
        }
    }
}
