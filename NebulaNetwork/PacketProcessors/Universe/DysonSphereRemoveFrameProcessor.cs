using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using System.Collections.Generic;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    internal class DysonSphereRemoveFrameProcessor : PacketProcessor<DysonSphereRemoveFramePacket>
    {
        public override void ProcessPacket(DysonSphereRemoveFramePacket packet, NebulaConnection conn)
        {
            DysonSphereLayer layer = GameMain.data.dysonSpheres[packet.StarIndex]?.GetLayer(packet.LayerId);
            if (layer == null)
            {
                return;
            }
            using (Multiplayer.Session.DysonSpheres.IsIncomingRequest.On())
            {
                if (!Check(layer, packet))
                {
                    Log.Warn($"Cannnot remove frame[{packet.FrameId}] on layer[{layer.id}], starIndex[{packet.StarIndex}]");
                    Multiplayer.Session.DysonSpheres.HandleDesync(packet.StarIndex, conn);
                    return;
                }
                //No need to remove if the frame is already null
                if (layer.framePool[packet.FrameId] != null)
                {
                    layer.RemoveDysonFrame(packet.FrameId);
                    NebulaWorld.Universe.DysonSphereManager.ClearSelection(packet.StarIndex, layer.id);
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
            DysonFrame frame = layer.framePool[packet.FrameId];
            if (frame == null)
            {
                //Sender and receiver are in the same state, so it's ok to pass
                return true;
            }
            //Make sure that shells connected to the frame are removed first.
            //UIDysonBrush_Remove.DeleteSelectedNode() remove frames first, so we need to remove shells here.
            List<int> delShellList = new List<int>();
            foreach (DysonShell shell in frame.nodeA.shells)
            {
                if (shell.frames.Contains(frame) && !delShellList.Contains(shell.id))
                {
                    delShellList.Add(shell.id);
                }
            }
            foreach (DysonShell shell in frame.nodeB.shells)
            {
                if (shell.frames.Contains(frame) && !delShellList.Contains(shell.id))
                {
                    delShellList.Add(shell.id);
                }
            }
            foreach (int shellId in delShellList)
            {
                layer.RemoveDysonShell(shellId);
            }
            return true;
        }
    }
}
