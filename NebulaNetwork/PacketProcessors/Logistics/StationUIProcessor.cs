using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets;
using NebulaWorld;
using UnityEngine;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class StationUIProcessor: PacketProcessor<StationUI>
    {
        public override void ProcessPacket(StationUI packet, NebulaConnection conn)
        {
            SimulatedWorld.OnStationUIChange(packet);
        }
    }
}
