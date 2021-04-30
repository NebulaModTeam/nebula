using NebulaModel.Attributes;
using System;

namespace NebulaModel
{
    [System.Serializable]
    public class MultiplayerOptions : ICloneable
    {
        [UIControl("Host Port")]
        public ushort HostPort { get; set; } = 8469;

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
