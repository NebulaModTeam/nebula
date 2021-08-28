using NebulaModel.DataStructures;
using NebulaModel.Packets.Universe;
using System;
using System.Collections.Generic;

namespace NebulaWorld.Universe
{
    public class DysonSphereManager : IDisposable
    {
        public readonly ToggleSwitch IsIncomingRequest = new ToggleSwitch();
        public readonly ToggleSwitch IncomingDysonSwarmPacket = new ToggleSwitch();

        public List<DysonSphereAddFramePacket> QueuedAddFramePackets = new List<DysonSphereAddFramePacket>();

        public DysonSphereManager()
        {
            QueuedAddFramePackets = new List<DysonSphereAddFramePacket>();
        }

        public void Dispose()
        {
            QueuedAddFramePackets = null;
        }

        public bool CanCreateFrame(int node1, int node2, DysonSphereLayer dsl)
        {
            if (dsl == null)
            {
                return false;
            }
            if ((ulong)node1 >= (ulong)((long)dsl.nodeCursor))
            {
                return false;
            }
            if ((ulong)node2 >= (ulong)((long)dsl.nodeCursor))
            {
                return false;
            }
            if (dsl.nodePool[node1] == null || dsl.nodePool[node2] == null)
            {
                return false;
            }
            return true;
        }

        public bool CanRemoveFrame(int frameId, DysonSphereLayer dsl)
        {
            return dsl != null && dsl.framePool[frameId] != null && dsl.framePool[frameId].nodeA.frames != null && dsl.framePool[frameId].nodeB.frames != null;
        }

        public bool CanRemoveShell(int shellId, DysonSphereLayer dsl)
        {
            if (dsl?.shellPool[shellId]?.nodes != null)
            {
                foreach (DysonNode dysonNode in dsl.shellPool[shellId].nodes)
                {
                    if (dysonNode == null)
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }
}
