using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.PowerTower;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaNetwork.PacketProcessors.Factory.PowerTower
{
    [RegisterPacketProcessor]
    public class PowerTowerUserLoadingRequestProcessor : PacketProcessor<PowerTowerUserLoadingRequest>
    {
        public override void ProcessPacket(PowerTowerUserLoadingRequest packet, NebulaConnection conn)
        {
            if (IsClient) return;

            PlanetFactory factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
            
            if (factory?.powerSystem != null)
            {
                PowerNetwork pNet = factory.powerSystem.netPool[packet.NetId];

                if (packet.Charging)
                {
                    PowerTowerManager.AddExtraDemand(packet.PlanetId, packet.NetId, packet.NodeId, packet.PowerAmount);
                }
                else
                {
                    PowerTowerManager.RemExtraDemand(packet.PlanetId, packet.NetId, packet.NodeId);
                }

                LocalPlayer.SendPacketToStar(new PowerTowerUserLoadingResponse(packet.PlanetId,
                        packet.NetId,
                        packet.NodeId,
                        packet.PowerAmount,
                        pNet.energyCapacity,
                        pNet.energyRequired,
                        pNet.energyServed,
                        pNet.energyAccumulated,
                        pNet.energyExchanged,
                        packet.Charging),
                    GameMain.galaxy.PlanetById(packet.PlanetId).star.id);
            }
        }
    }
}