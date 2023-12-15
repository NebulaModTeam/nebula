#region

using System;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe.Editor;
using NebulaWorld;
using NebulaWorld.Universe;

#endregion

namespace NebulaNetwork.PacketProcessors.Universe.Editor;

[RegisterPacketProcessor]
internal class DysonSwarmRemoveOrbitProcessor : PacketProcessor<DysonSwarmRemoveOrbitPacket>
{
    protected override void ProcessPacket(DysonSwarmRemoveOrbitPacket packet, NebulaConnection conn)
    {
        var sphere = GameMain.data.dysonSpheres[packet.StarIndex];
        if (sphere == null)
        {
            return;
        }
        using (Multiplayer.Session.DysonSpheres.IncomingDysonSwarmPacket.On())
        {
            switch (packet.Event)
            {
                case SwarmRemoveOrbitEvent.Remove:
                    sphere.swarm.RemoveOrbit(packet.OrbitId);
                    break;
                case SwarmRemoveOrbitEvent.Enable:
                case SwarmRemoveOrbitEvent.Disable:
                    sphere.swarm.SetOrbitEnable(packet.OrbitId, packet.Event == SwarmRemoveOrbitEvent.Enable);
                    break;
                case SwarmRemoveOrbitEvent.RemoveSails:
                    sphere.swarm.RemoveSailsByOrbit(packet.OrbitId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(packet), "Unknown SwarmRemoveOrbitEvent type: " + packet.Event);
            }
            DysonSphereManager.ClearSelection(packet.StarIndex);
        }
        if (IsHost)
        {
            Multiplayer.Session.DysonSpheres.SendPacketToDysonSphereExcept(packet, packet.StarIndex, conn);
        }
    }
}
