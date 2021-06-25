using NebulaModel.Attributes;
using System;
using System.ComponentModel;

namespace NebulaModel
{
    [System.Serializable]
    public class MultiplayerOptions : ICloneable
    {
        [DisplayName("Nickname")]
        public string Nickname { get; set; } = string.Empty;

        [DisplayName("Mecha Color Red")]
        [UIRange(0, 255, true)]
        public float MechaColorR { get; set; } = 255;

        [DisplayName("Mecha Color Green")]
        [UIRange(0, 255, true)]
        public float MechaColorG { get; set; } = 174;

        [DisplayName("Mecha Color Blue")]
        [UIRange(0, 255, true)]
        public float MechaColorB { get; set; } = 61;

        [DisplayName("Host Port")]
        [UIRange(1, ushort.MaxValue)]
        public ushort HostPort { get; set; } = 8469;

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
