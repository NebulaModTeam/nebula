#region

using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Universe;

[RegisterPacketProcessor]
internal class DysonSphereAddFrameProcessor : PacketProcessor<DysonSphereAddFramePacket>
{
    public override void ProcessPacket(DysonSphereAddFramePacket packet, NebulaConnection conn)
    {
        var layer = GameMain.data.dysonSpheres[packet.StarIndex]?.GetLayer(packet.LayerId);
        if (layer == null)
        {
            return;
        }
        using (Multiplayer.Session.DysonSpheres.IsIncomingRequest.On())
        {
            var frameId = layer.frameRecycleCursor > 0 ? layer.frameRecycle[layer.frameRecycleCursor - 1] : layer.frameCursor;
            if (frameId != packet.FrameId ||
                layer.NewDysonFrame(packet.ProtoId, packet.NodeAId, packet.NodeBId, packet.Euler) == 0)
            {
                Log.Warn($"Cannnot add frame[{packet.FrameId}] on layer[{layer.id}], starIndex[{packet.StarIndex}]");
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
