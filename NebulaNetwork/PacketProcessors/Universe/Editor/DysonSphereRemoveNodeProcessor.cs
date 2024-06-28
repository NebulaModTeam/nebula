#region

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
internal class DysonSphereRemoveNodeProcessor : PacketProcessor<DysonSphereRemoveNodePacket>
{
    protected override void ProcessPacket(DysonSphereRemoveNodePacket packet, NebulaConnection conn)
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
                Log.Warn($"Cannot remove node[{packet.NodeId}] on layer[{layer.id}], starIndex[{packet.StarIndex}]");
                Multiplayer.Session.DysonSpheres.HandleDesync(packet.StarIndex, conn);
                return;
            }
            //No need to remove if the node is already null
            if (layer.nodePool[packet.NodeId] != null)
            {
                layer.RemoveDysonNode(packet.NodeId);
                DysonSphereManager.ClearSelection(packet.StarIndex, layer.id);
            }
        }
        if (IsHost)
        {
            Multiplayer.Session.DysonSpheres.SendPacketToDysonSphereExcept(packet, packet.StarIndex, conn);
        }
    }

    private static bool Check(DysonSphereLayer layer, DysonSphereRemoveNodePacket packet)
    {
        if (packet.NodeId < 1 || packet.NodeId >= layer.nodeCursor)
        {
            return false;
        }
        var node = layer.nodePool[packet.NodeId];
        if (node == null)
        {
            //Sender and receiver are in the same state, so it's ok to pass
            return true;
        }
        //Make sure that shells and frames connected to the node are removed first.
        return node.frames.Count <= 0;
    }
}
