using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;

/*
 * used to decrease the stored energy of an ILS when a ship departs
 * only sent by server and processed by clients
 */
namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    public class ILSEnergyConsumeNotificationProcessor : PacketProcessor<ILSEnergyConsumeNotification>
    {
        public override void ProcessPacket(ILSEnergyConsumeNotification packet, NebulaConnection conn)
        {
            if (GameMain.data.galacticTransport.stationPool.Length > packet.stationGId && GameMain.data.galacticTransport.stationPool[packet.stationGId] != null)
            {
                StationComponent stationComponent = GameMain.data.galacticTransport.stationPool[packet.stationGId];
                stationComponent.energy -= packet.cost;
                if (stationComponent.energy < 0)
                {
                    stationComponent.energy = 0;
                }
            }
        }
    }
}
