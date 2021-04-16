using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using NebulaWorld;
using UnityEngine;

namespace NebulaClient.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class StationUIProcessor: IPacketProcessor<StationUI>
    {
        public void ProcessPacket(StationUI packet, NebulaConnection conn)
        {
            SimulatedWorld.OnStationUIChange(packet);
        }
    }
}
