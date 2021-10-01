using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.PowerTower;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Factory.PowerTower
{
    [RegisterPacketProcessor]
    public class PowerTowerUserLoadingRequestProcessor : PacketProcessor<PowerTowerUserLoadingRequest>
    {
        public override void ProcessPacket(PowerTowerUserLoadingRequest packet, NebulaConnection conn)
        {
            if (IsClient)
            {
                return;
            }

            PlanetFactory factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;

            if (factory?.powerSystem != null)
            {
                PowerNetwork pNet = factory.powerSystem.netPool[packet.NetId];

                if (packet.Charging)
                {
                    Multiplayer.Session.PowerTowers.AddExtraDemand(packet.PlanetId, packet.NetId, packet.NodeId, packet.PowerAmount);
                }
                else
                {
                    Multiplayer.Session.PowerTowers.RemExtraDemand(packet.PlanetId, packet.NetId, packet.NodeId);
                }

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
        }
    }
}