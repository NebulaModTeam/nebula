#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using NebulaWorld.Logistics;

#endregion

namespace NebulaNetwork.PacketProcessors.Logistics;

[RegisterPacketProcessor]
public class ILSIdleShipBackToWorkProcessor : PacketProcessor<ILSIdleShipBackToWork>
{
    protected override void ProcessPacket(ILSIdleShipBackToWork packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            return;
        }

        if (!IsClient)
        {
            return;
        }
        using (Multiplayer.Session.Factories.IsIncomingRequest.On())
        {
            ILSShipManager.IdleShipGetToWork(packet);
        }
    }
}
