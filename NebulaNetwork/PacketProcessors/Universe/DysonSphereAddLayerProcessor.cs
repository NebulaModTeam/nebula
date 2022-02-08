using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    public class DysonSphereAddLayerProcessor : PacketProcessor<DysonSphereAddLayerPacket>
    {
        public override void ProcessPacket(DysonSphereAddLayerPacket packet, NebulaConnection conn)
        {
            DysonSphere sphere = GameMain.data.dysonSpheres[packet.StarIndex];
            if (sphere == null)
            {
                return;
            }
            using (Multiplayer.Session.DysonSpheres.IsIncomingRequest.On())
            {
                if (packet.LayerId != sphere.QueryLayerId())
                {
                    Multiplayer.Session.DysonSpheres.HandleDesync(packet.StarIndex, conn);
                    return;
                }
                sphere.AddLayer(packet.OrbitRadius, packet.OrbitRotation.ToQuaternion(), packet.OrbitAngularSpeed);
            }
            if (IsHost)
            {
                Multiplayer.Session.DysonSpheres.SendPacketToDysonSphereExcept(packet, packet.StarIndex, conn);
            }
        }
    }
}
