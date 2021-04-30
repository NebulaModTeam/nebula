using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Universe;
using NebulaWorld.Universe;

namespace NebulaClient.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSphereRemoveFrameProcessor : IPacketProcessor<DysonSphereRemoveFramePacket>
    {
        public void ProcessPacket(DysonSphereRemoveFramePacket packet, NebulaConnection conn)
        {
            using (DysonSphere_Manager.IncomingDysonSpherePacket.On())
            {
                DysonSphereLayer dsl = GameMain.data.dysonSpheres[packet.StarIndex]?.GetLayer(packet.LayerId);
                if (DysonSphere_Manager.CanRemoveFrame(packet.FrameId, dsl))
                {
                    dsl.RemoveDysonFrame(packet.FrameId);
                }
            }
        }
    }
}
