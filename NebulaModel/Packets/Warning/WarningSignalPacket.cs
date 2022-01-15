using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NebulaModel.Packets.Warning
{
    public class WarningSignalPacket
    {
        public int SignalCount { get; set; }
        public int[] Signals { get; set; }
        public int[] Counts { get; set; }
        public int Tick { get; set; }

        public WarningSignalPacket() { }
    }
}
