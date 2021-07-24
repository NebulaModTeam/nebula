using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld.Universe;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSphereRemoveShellProcessor : PacketProcessor<DysonSphereRemoveShellPacket>
    {
        public override void ProcessPacket(DysonSphereRemoveShellPacket packet, NebulaConnection conn)
        {
            using (DysonSphere_Manager.IncomingDysonSpherePacket.On())
            {
                DysonSphereLayer dsl = GameMain.data.dysonSpheres[packet.StarIndex]?.GetLayer(packet.LayerId);
                if (DysonSphere_Manager.CanRemoveShell(packet.ShellId, dsl))
                {
                    dsl.RemoveDysonShell(packet.ShellId);
                }
            }
        }
    }
}
