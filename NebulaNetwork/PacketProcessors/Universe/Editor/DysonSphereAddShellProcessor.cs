#region

using System.Collections.Generic;
using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Universe;

[RegisterPacketProcessor]
internal class DysonSphereAddShellProcessor : PacketProcessor<DysonSphereAddShellPacket>
{
    public override void ProcessPacket(DysonSphereAddShellPacket packet, NebulaConnection conn)
    {
        var layer = GameMain.data.dysonSpheres[packet.StarIndex]?.GetLayer(packet.LayerId);
        if (layer == null)
        {
            return;
        }
        using (Multiplayer.Session.DysonSpheres.IsIncomingRequest.On())
        {
            var shellId = layer.shellRecycleCursor > 0 ? layer.shellRecycle[layer.shellRecycleCursor - 1] : layer.shellCursor;
            if (shellId != packet.ShellId || layer.NewDysonShell(packet.ProtoId, new List<int>(packet.NodeIds)) == 0)
            {
                Log.Warn($"Cannnot add shell[{packet.ShellId}] on layer[{layer.id}], starIndex[{packet.StarIndex}]");
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
