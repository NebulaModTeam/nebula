using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    public class DysonSphereEditLayerProcessor : PacketProcessor<DysonSphereEditLayerPacket>
    {
        public override void ProcessPacket(DysonSphereEditLayerPacket packet, NebulaConnection conn)
        {
            DysonSphere sphere = GameMain.data.dysonSpheres[packet.StarIndex];
            if (sphere == null)
            {
                return;
            }
            using (Multiplayer.Session.DysonSpheres.IsIncomingRequest.On())
            {
                DysonSphereLayer layer = sphere.GetLayer(packet.LayerId);
                if (layer == null)
                {
                    Multiplayer.Session.DysonSpheres.HandleDesync(packet.StarIndex, conn);
                    return;
                }
                layer.targetOrbitRotation = packet.OrbitRotation.ToQuaternion();
                layer.InitOrbitRotation(layer.orbitRotation, layer.targetOrbitRotation);
            }
            if (IsHost)
            {
                Multiplayer.Session.DysonSpheres.SendPacketToDysonSphereExcept(packet, packet.StarIndex, conn);
            }
        }
    }
}
