using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using UnityEngine;

namespace NebulaHost.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    public class ILSRequestShipDockPosProcessor: IPacketProcessor<ILSRequestShipDockPos>
    {
        private PlayerManager playerManager;
        public ILSRequestShipDockPosProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }
        public void ProcessPacket(ILSRequestShipDockPos packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player != null && GameMain.data.galacticTransport.stationCapacity > packet.stationGId)
            {
                ILSRequestShipDockPos packet2 = new ILSRequestShipDockPos(packet.stationGId, GameMain.data.galacticTransport.stationPool[packet.stationGId].shipDockPos);
                player.SendPacket(packet2);
            }
        }
    }
}
