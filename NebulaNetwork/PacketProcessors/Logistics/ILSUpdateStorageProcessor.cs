#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld.Logistics;

#endregion

namespace NebulaNetwork.PacketProcessors.Logistics;

[RegisterPacketProcessor]
public class ILSUpdateStorageProcessor : PacketProcessor<ILSUpdateStorage>
{
    protected override void ProcessPacket(ILSUpdateStorage packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            return;
        }

        ILSShipManager.UpdateStorage(packet);
    }
}
