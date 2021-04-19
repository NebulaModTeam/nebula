using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Universe;
using NebulaWorld.Universe;

namespace NebulaClient.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSphereRemoveShellProcessor : IPacketProcessor<DysonSphereRemoveShellPacket>
    {
        public void ProcessPacket(DysonSphereRemoveShellPacket packet, NebulaConnection conn)
        {
            Log.Info($"Processing DysonSphere remove shell notification for system {GameMain.data.galaxy.stars[packet.StarIndex].name} (Index: {GameMain.data.galaxy.stars[packet.StarIndex].index})");
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
