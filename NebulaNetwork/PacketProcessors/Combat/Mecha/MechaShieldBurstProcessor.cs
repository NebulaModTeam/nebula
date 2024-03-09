#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.Mecha;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.Mecha;

[RegisterPacketProcessor]
public class MechaShieldBurstProcessor : PacketProcessor<MechaShieldBurstPacket>
{
    protected override void ProcessPacket(MechaShieldBurstPacket packet, NebulaConnection conn)
    {
        using (Multiplayer.Session.Combat.IsIncomingRequest.On())
        {
            Multiplayer.Session.Combat.ShieldBurst(packet);
        }
    }
}
