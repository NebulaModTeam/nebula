#region

using System.Collections.Generic;
using System.Linq;
using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe.Editor;
using NebulaWorld;
using NebulaWorld.Universe;

#endregion

namespace NebulaNetwork.PacketProcessors.Universe.Editor;

[RegisterPacketProcessor]
internal class DysonSphereRemoveFrameProcessor : PacketProcessor<DysonSphereRemoveFramePacket>
{
    protected override void ProcessPacket(DysonSphereRemoveFramePacket packet, NebulaConnection conn)
    {
        var layer = GameMain.data.dysonSpheres[packet.StarIndex]?.GetLayer(packet.LayerId);
        if (layer == null)
        {
            return;
        }
        using (Multiplayer.Session.DysonSpheres.IsIncomingRequest.On())
        {
            if (!Check(layer, packet))
            {
                Log.Warn($"Cannot remove frame[{packet.FrameId}] on layer[{layer.id}], starIndex[{packet.StarIndex}]");
                Multiplayer.Session.DysonSpheres.HandleDesync(packet.StarIndex, conn);
                return;
            }
            //No need to remove if the frame is already null
            if (layer.framePool[packet.FrameId] != null)
            {
                layer.RemoveDysonFrame(packet.FrameId);
                DysonSphereManager.ClearSelection(packet.StarIndex, layer.id);
            }
        }
        if (IsHost)
        {
            Multiplayer.Session.DysonSpheres.SendPacketToDysonSphereExcept(packet, packet.StarIndex, conn);
        }
    }

    private static bool Check(DysonSphereLayer layer, DysonSphereRemoveFramePacket packet)
    {
        if (packet.FrameId < 1 || packet.FrameId >= layer.frameCursor)
        {
            return false;
        }
        var frame = layer.framePool[packet.FrameId];
        if (frame == null)
        {
            //Sender and receiver are in the same state, so it's ok to pass
            return true;
        }
        //Make sure that shells connected to the frame are removed first.
        //UIDysonBrush_Remove.DeleteSelectedNode() remove frames first, so we need to remove shells here.
        var delShellList = new List<int>();
        foreach (var shell in frame.nodeA.shells.Where(shell => shell.frames.Contains(frame) && !delShellList.Contains(shell.id)))
        {
            delShellList.Add(shell.id);
        }
        foreach (var shell in frame.nodeB.shells.Where(shell => shell.frames.Contains(frame) && !delShellList.Contains(shell.id)))
        {
            delShellList.Add(shell.id);
        }
        foreach (var shellId in delShellList)
        {
            layer.RemoveDysonShell(shellId);
        }
        return true;
    }
}
