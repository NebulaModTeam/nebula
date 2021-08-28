using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.PowerTower;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaNetwork.PacketProcessors.Factory.PowerTower
{
    [RegisterPacketProcessor]
    class PowerTowerUserLoadResponseProcessor : PacketProcessor<PowerTowerUserLoadingResponse>
    {
        public override void ProcessPacket(PowerTowerUserLoadingResponse packet, NebulaConnection conn)
        {
            PlanetFactory factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
            if (factory != null && factory.powerSystem != null)
            {
                PowerNetwork pNet = factory.powerSystem.netPool[packet.NetId];

                if (packet.Charging)
                {
                    Multiplayer.Session.PowerTowers.AddExtraDemand(packet.PlanetId, packet.NetId, packet.NodeId, packet.PowerAmount);
                    if (IsClient)
                    {
                        if (Multiplayer.Session.PowerTowers.DidRequest(packet.PlanetId, packet.NetId, packet.NodeId))
                        {
                            int baseDemand = factory.powerSystem.nodePool[packet.NodeId].workEnergyPerTick - factory.powerSystem.nodePool[packet.NodeId].idleEnergyPerTick;
                            float mult = factory.powerSystem.networkServes[packet.NetId];
                            Multiplayer.Session.PowerTowers.PlayerChargeAmount += (int)(mult * (float)baseDemand);
                        }
                    }
                }
                else
                {
                    Multiplayer.Session.PowerTowers.RemExtraDemand(packet.PlanetId, packet.NetId, packet.NodeId);
                }

                if (IsHost)
                {
                    Multiplayer.Session.Network.SendPacketToStar(new PowerTowerUserLoadingResponse(packet.PlanetId,
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
                else
                {
                    pNet.energyCapacity = packet.EnergyCapacity;
                    pNet.energyRequired = packet.EnergyRequired;
                    pNet.energyAccumulated = packet.EnergyAccumulated;
                    pNet.energyExchanged = packet.EnergyExchanged;
                    pNet.energyServed = packet.EnergyServed;
                }

            }
        }
    }
}
