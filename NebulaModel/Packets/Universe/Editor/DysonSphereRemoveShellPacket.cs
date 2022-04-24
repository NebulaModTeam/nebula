﻿namespace NebulaModel.Packets.Universe
{
    public class DysonSphereRemoveShellPacket
    {
        public int StarIndex { get; set; }
        public int LayerId { get; set; }
        public int ShellId { get; set; }

        public DysonSphereRemoveShellPacket() { }
        public DysonSphereRemoveShellPacket(int starIndex, int layerId, int shellId)
        {
            StarIndex = starIndex;
            LayerId = layerId;
            ShellId = shellId;
        }
    }
}
