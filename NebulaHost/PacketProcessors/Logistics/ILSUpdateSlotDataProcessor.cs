using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using NebulaWorld.Logistics;

namespace NebulaHost.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class ILSUpdateSlotDataProcessor : IPacketProcessor<ILSUpdateSlotData>
    {
        private PlayerManager playerManager;
        public ILSUpdateSlotDataProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }
        public void ProcessPacket(ILSUpdateSlotData packet, NebulaConnection conn)
        {
            PlanetData pData = null;
            // PLS
            if(packet.planetId != 0)
            {
                pData = GameMain.galaxy.PlanetById(packet.planetId);
            }
            else // ILS
            {
                if (packet.stationGId < GameMain.data.galacticTransport.stationPool.Length)
                {
                    StationComponent stationComponent = GameMain.data.galacticTransport.stationPool[packet.stationGId];
                    if(stationComponent != null)
                    {
                        pData = GameMain.galaxy.PlanetById(stationComponent.planetId);
                    }
                }
            }

            if(pData != null)
            {
                playerManager.SendPacketToStar(packet, pData.star.id);
            }
            ILSShipManager.UpdateSlotData(packet);
        }
    }
}
