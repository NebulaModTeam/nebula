using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld.Universe;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSphereRemoveFrameProcessor : PacketProcessor<DysonSphereRemoveFramePacket>
    {
        public override void ProcessPacket(DysonSphereRemoveFramePacket packet, NebulaConnection conn)
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
