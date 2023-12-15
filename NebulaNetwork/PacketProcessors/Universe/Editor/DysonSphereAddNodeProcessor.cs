#region

using NebulaAPI.DataStructures;
using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe.Editor;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Universe.Editor;

[RegisterPacketProcessor]
internal class DysonSphereAddNodeProcessor : PacketProcessor<DysonSphereAddNodePacket>
{
    protected override void ProcessPacket(DysonSphereAddNodePacket packet, NebulaConnection conn)
    {
        var layer = GameMain.data.dysonSpheres[packet.StarIndex]?.GetLayer(packet.LayerId);
        if (layer == null)
        {
            return;
        }
        using (Multiplayer.Session.DysonSpheres.IsIncomingRequest.On())
        {
            var nodeId = layer.nodeRecycleCursor > 0 ? layer.nodeRecycle[layer.nodeRecycleCursor - 1] : layer.nodeCursor;
            if (nodeId != packet.NodeId || layer.NewDysonNode(packet.NodeProtoId, packet.Position.ToVector3()) == 0)
            {
                Log.Warn($"Cannnot add node[{packet.NodeId}] on layer[{layer.id}], starIndex[{packet.StarIndex}]");
                Multiplayer.Session.DysonSpheres.HandleDesync(packet.StarIndex, conn);
                return;
            }
        }
        if (IsHost)
        {
            Multiplayer.Session.DysonSpheres.SendPacketToDysonSphereExcept(packet, packet.StarIndex, conn);
        }
    }
}
