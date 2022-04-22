using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using UnityEngine;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    public class DysonSphereColorChangeProcessor : PacketProcessor<DysonSphereColorChangePacket>
    {
        public override void ProcessPacket(DysonSphereColorChangePacket packet, NebulaConnection conn)
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
                Color32 color = packet.Color.ToColor32();
                switch (packet.Type) 
                {
                    case DysonSphereColorChangePacket.ComponentType.Node:
                        DysonNode node = packet.Index < layer.nodeCursor ? layer.nodePool[packet.Index] : null;
                        if (node != null)
                        {
                            node.color = color;
                            sphere.UpdateColor(node);
                        }
                        break;

                    case DysonSphereColorChangePacket.ComponentType.Frame:
                        DysonFrame frame = packet.Index < layer.frameCursor ? layer.framePool[packet.Index] : null;
                        if (frame != null)
                        {
                            frame.color = color;
                            sphere.UpdateColor(frame);
                        }
                        break;

                    case DysonSphereColorChangePacket.ComponentType.Shell:
                        DysonShell shell = packet.Index < layer.shellCursor ? layer.shellPool[packet.Index] : null;
                        if (shell != null)
                        {
                            shell.color = color;
                        }
                        break;
                }
            }
            if (IsHost)
            {
                Multiplayer.Session.DysonSpheres.SendPacketToDysonSphereExcept(packet, packet.StarIndex, conn);
            }
        }
    }
}
