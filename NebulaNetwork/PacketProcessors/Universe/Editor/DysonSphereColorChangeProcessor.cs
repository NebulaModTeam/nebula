#region

using System;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe.Editor;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Universe.Editor;

[RegisterPacketProcessor]
public class DysonSphereColorChangeProcessor : PacketProcessor<DysonSphereColorChangePacket>
{
    protected override void ProcessPacket(DysonSphereColorChangePacket packet, NebulaConnection conn)
    {
        var sphere = GameMain.data.dysonSpheres[packet.StarIndex];
        if (sphere == null)
        {
            return;
        }
        using (Multiplayer.Session.DysonSpheres.IsIncomingRequest.On())
        {
            var layer = sphere.GetLayer(packet.LayerId);
            if (layer == null)
            {
                Multiplayer.Session.DysonSpheres.HandleDesync(packet.StarIndex, conn);
                return;
            }
            var color = packet.Color.ToColor32();
            switch (packet.Type)
            {
                case DysonSphereColorChangePacket.ComponentType.Node:
                    var node = packet.Index < layer.nodeCursor ? layer.nodePool[packet.Index] : null;
                    if (node != null)
                    {
                        node.color = color;
                        sphere.UpdateColor(node);
                    }
                    break;

                case DysonSphereColorChangePacket.ComponentType.Frame:
                    var frame = packet.Index < layer.frameCursor ? layer.framePool[packet.Index] : null;
                    if (frame != null)
                    {
                        frame.color = color;
                        sphere.UpdateColor(frame);
                    }
                    break;

                case DysonSphereColorChangePacket.ComponentType.Shell:
                    var shell = packet.Index < layer.shellCursor ? layer.shellPool[packet.Index] : null;
                    if (shell != null)
                    {
                        shell.color = color;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(packet), "Unknown DysonSphereColorChangePacket type: " + packet.Type);
            }
        }
        if (IsHost)
        {
            Multiplayer.Session.DysonSpheres.SendPacketToDysonSphereExcept(packet, packet.StarIndex, conn);
        }
    }
}
