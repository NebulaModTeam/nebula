using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    internal class DysonSwarmRemoveOrbitProcessor : PacketProcessor<DysonSwarmRemoveOrbitPacket>
    {
        public override void ProcessPacket(DysonSwarmRemoveOrbitPacket packet, NebulaConnection conn)
        {
            DysonSphere sphere = GameMain.data.dysonSpheres[packet.StarIndex];
            if (sphere == null)
            {
                return;
            }
            using (Multiplayer.Session.DysonSpheres.IncomingDysonSwarmPacket.On())
            {
                if (packet.Event == SwarmRemoveOrbitEvent.Remove)
                {
                    sphere.swarm.RemoveOrbit(packet.OrbitId);
                }
                else if (packet.Event == SwarmRemoveOrbitEvent.Enable || packet.Event == SwarmRemoveOrbitEvent.Disable)
                {
                    sphere.swarm.SetOrbitEnable(packet.OrbitId, packet.Event == SwarmRemoveOrbitEvent.Enable);
                }
                NebulaWorld.Universe.DysonSphereManager.ClearSelection(packet.StarIndex);
            }
            if (IsHost)
            {
                Multiplayer.Session.DysonSpheres.SendPacketToDysonSphereExcept(packet, packet.StarIndex, conn);
            }
        }
    }
}
