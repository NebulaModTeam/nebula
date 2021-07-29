using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld.Logistics;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class ILSUpdateSlotDataProcessor : PacketProcessor<ILSUpdateSlotData>
    {
        private PlayerManager playerManager;
        public ILSUpdateSlotDataProcessor()
        {
            playerManager = MultiplayerHostSession.Instance?.PlayerManager;
        }

        public override void ProcessPacket(ILSUpdateSlotData packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                PlanetData pData = null;
                // PLS
                if (packet.StationGId == 0)
                {
                    pData = GameMain.galaxy.PlanetById(packet.PlanetId);
                }
                else // ILS
                {
                    if (packet.StationGId < GameMain.data.galacticTransport.stationPool.Length)
                    {
                        StationComponent stationComponent = GameMain.data.galacticTransport.stationPool[packet.StationGId];
                        if (stationComponent != null)
                        {
                            pData = GameMain.galaxy.PlanetById(stationComponent.planetId);
                        }
                    }
                }

                if (pData != null)
                {
                    playerManager.SendPacketToStar(packet, pData.star.id);
                }

                ILSShipManager.UpdateSlotData(packet);
            }

            if (IsClient)
            {
                ILSShipManager.UpdateSlotData(packet);
            }
        }
    }
}
