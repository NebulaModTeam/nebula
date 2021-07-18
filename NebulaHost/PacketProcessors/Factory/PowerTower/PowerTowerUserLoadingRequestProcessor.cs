using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory.PowerTower;
using NebulaModel.Packets.Processors;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaHost.PacketProcessors.Factory.PowerTower
{
    [RegisterPacketProcessor]
    class PowerTowerUserLoadingRequestProcessor: IPacketProcessor<PowerTowerUserLoadingRequest>
    {
        public void ProcessPacket(PowerTowerUserLoadingRequest packet, NebulaConnection conn)
        {
            PlanetFactory factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
            if(factory != null && factory.powerSystem != null)
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
