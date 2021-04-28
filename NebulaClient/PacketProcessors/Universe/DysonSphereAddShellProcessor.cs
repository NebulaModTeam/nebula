using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Universe;
using NebulaWorld.Universe;
using System.Collections.Generic;

namespace NebulaClient.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSphereAddShellProcessor : IPacketProcessor<DysonSphereAddShellPacket>
    {

        public void ProcessPacket(DysonSphereAddShellPacket packet, NebulaConnection conn)
        {
            using (DysonSphere_Manager.IncomingDysonSpherePacket.On())
                GameMain.data.dysonSpheres[packet.StarIndex]?.GetLayer(packet.LayerId)?.NewDysonShell(packet.ProtoId, new List<int>(packet.NodeIds));
        }
    }
}

