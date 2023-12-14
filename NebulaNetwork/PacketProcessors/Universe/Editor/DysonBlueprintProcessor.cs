#region

using System.Text;
using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe.Editor;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Universe.Editor;

[RegisterPacketProcessor]
internal class DysonBlueprintProcessor : PacketProcessor<DysonBlueprintPacket>
{
    protected override void ProcessPacket(DysonBlueprintPacket packet, NebulaConnection conn)
    {
        var sphere = GameMain.data.dysonSpheres[packet.StarIndex];
        if (sphere == null)
        {
            return;
        }
        using (Multiplayer.Session.DysonSpheres.IsIncomingRequest.On())
        {
            var layer = sphere.GetLayer(packet.LayerId);
            var str64Data = Encoding.ASCII.GetString(packet.BinaryData);
            var err = new DysonBlueprintData().FromBase64String(str64Data, packet.BlueprintType, sphere, layer);
            if (err != DysonBlueprintDataIOError.OK)
            {
                Log.Warn($"DysonBlueprintData IO error: {err}");
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
