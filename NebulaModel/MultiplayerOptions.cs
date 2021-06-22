using NebulaModel.Attributes;
using System;
using System.ComponentModel;

namespace NebulaModel
{
    [System.Serializable]
    public class MultiplayerOptions : ICloneable
    {
        [DisplayName("Host Port")]
        [UIRange(1, ushort.MaxValue)]
        public ushort HostPort { get; set; } = 8469;

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
