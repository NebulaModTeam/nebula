using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory.PowerTower;
using NebulaModel.Packets;
using NebulaWorld.Factory;

namespace NebulaClient.PacketProcessors.Factory.PowerTower
{
    [RegisterPacketProcessor]
    class PowerTowerUserLoadResponseProcessor: PacketProcessor<PowerTowerUserLoadingResponse>
    {
        public override void ProcessPacket(PowerTowerUserLoadingResponse packet, NebulaConnection conn)
        {
            PlanetFactory factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
            if (factory != null && factory.powerSystem != null)
            {
                PowerNetwork pNet = factory.powerSystem.netPool[packet.NetId];

                if (packet.Charging)
                {
                    PowerTowerManager.AddExtraDemand(packet.PlanetId, packet.NetId, packet.NodeId, packet.PowerAmount);
                    if(PowerTowerManager.DidRequest(packet.PlanetId, packet.NetId, packet.NodeId))
                    {
                        int baseDemand = factory.powerSystem.nodePool[packet.NodeId].workEnergyPerTick - factory.powerSystem.nodePool[packet.NodeId].idleEnergyPerTick;
                        float mult = factory.powerSystem.networkServes[packet.NetId];
                        PowerTowerManager.PlayerChargeAmount += (int)(mult * (float)baseDemand);
                    }
                }
                else
                {
                    PowerTowerManager.RemExtraDemand(packet.PlanetId, packet.NetId, packet.NodeId);
                }

                pNet.energyCapacity = packet.EnergyCapacity;
                pNet.energyRequired = packet.EnergyRequired;
                pNet.energyAccumulated = packet.EnergyAccumulated;
                pNet.energyExchanged = packet.EnergyExchanged;
                pNet.energyServed = packet.EnergyServed;
            }
        }
    }
}
