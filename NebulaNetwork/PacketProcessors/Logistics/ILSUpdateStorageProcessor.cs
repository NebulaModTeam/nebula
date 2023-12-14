#region

using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Logistics;

[RegisterPacketProcessor]
public class ILSUpdateStorageProcessor : PacketProcessor<ILSUpdateStorage>
{
    public override void ProcessPacket(ILSUpdateStorage packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            return;
        }

        Multiplayer.Session.Ships.UpdateStorage(packet);
    }
}
