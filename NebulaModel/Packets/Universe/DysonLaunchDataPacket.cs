using NebulaAPI;
using NebulaModel.DataStructures;

namespace NebulaModel.Packets.Universe
{
    [HidePacketInDebugLogs]
    public class DysonLaunchDataPacket
    {
        public DysonLaunchData Data { get; set; }

        public DysonLaunchDataPacket() { }
        public DysonLaunchDataPacket(DysonLaunchData data)
        {
            Data = data;
        }
    }
}
