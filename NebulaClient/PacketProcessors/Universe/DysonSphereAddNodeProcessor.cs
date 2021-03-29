using NebulaModel.Networking;
using NebulaModel.Packets.Universe;
using NebulaModel.Logger;
using NebulaModel.DataStructures;
using NebulaModel.Packets.Processors;
using NebulaWorld.Universe;
using NebulaModel.Attributes;

namespace NebulaClient.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSphereAddNodeProcessor : IPacketProcessor<DysonSphereAddNodePacket>
    {
        public void ProcessPacket(DysonSphereAddNodePacket packet, NebulaConnection conn)
        {
            Log.Info($"Processing DysonSphere Add Node notification for system {GameMain.data.galaxy.stars[packet.StarIndex].name} (Index: {GameMain.data.galaxy.stars[packet.StarIndex].index})");
            DysonSphere_Manager.IncomingDysonSpherePacket = true;
            int? addedID = GameMain.data.dysonSpheres[packet.StarIndex]?.GetLayer(packet.LayerId)?.NewDysonNode(packet.NodeProtoId, DataStructureExtensions.ToUnity(packet.Position));
            //Try to add frames that failed due to the missing nodes
            DysonSphereLayer dsl = GameMain.data.dysonSpheres[packet.StarIndex]?.GetLayer(packet.LayerId);
            DysonSphereAddFramePacket queuedPacked;
            for (int i = DysonSphere_Manager.QueuedAddFramePackets.Count - 1; i >= 0; i--)
            {
                queuedPacked = DysonSphere_Manager.QueuedAddFramePackets[i];
                if (dsl?.nodePool[queuedPacked.NodeAId]?.id != 0 && dsl?.nodePool[queuedPacked.NodeBId]?.id != 0)
                {
                    dsl.NewDysonFrame(queuedPacked.ProtoId, queuedPacked.NodeAId, queuedPacked.NodeBId, queuedPacked.Euler);
                    DysonSphere_Manager.QueuedAddFramePackets.RemoveAt(i);
                }
            }
            DysonSphere_Manager.IncomingDysonSpherePacket = false;
        }
    }
}
