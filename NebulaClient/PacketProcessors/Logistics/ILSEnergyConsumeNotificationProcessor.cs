using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;

/*
 * used to decrease the stored energy of an ILS when a ship departs
 * only sent by server and processed by clients
 */
namespace NebulaClient.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    public class ILSEnergyConsumeNotificationProcessor: IPacketProcessor<ILSEnergyConsumeNotification>
    {
        public void ProcessPacket(ILSEnergyConsumeNotification packet, NebulaConnection conn)
        {
            if(GameMain.data.galacticTransport.stationPool.Length > packet.stationGId && GameMain.data.galacticTransport.stationPool[packet.stationGId] != null)
            {
                StationComponent stationComponent = GameMain.data.galacticTransport.stationPool[packet.stationGId];
                stationComponent.energy -= packet.cost;
                if(stationComponent.energy < 0)
                {
                    stationComponent.energy = 0;
                }
            }
        }
    }
}
