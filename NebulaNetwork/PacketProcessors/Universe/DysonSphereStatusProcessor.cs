using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    internal class DysonSphereStatusProcessor : PacketProcessor<DysonSphereStatusPacket>
    {
        public override void ProcessPacket(DysonSphereStatusPacket packet, NebulaConnection conn)
        {
            DysonSphere dysonSphere = GameMain.data.dysonSpheres[packet.StarIndex];
            if (IsHost || dysonSphere == null)
            {
                return;
            }            
            dysonSphere.grossRadius = packet.GrossRadius;
            dysonSphere.energyReqCurrentTick = packet.EnergyReqCurrentTick;
            dysonSphere.energyGenCurrentTick = packet.EnergyGenCurrentTick;
        }
    }
}