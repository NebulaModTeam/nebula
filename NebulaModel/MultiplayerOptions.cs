using NebulaModel.Attributes;
using System;
using System.ComponentModel;

namespace NebulaModel
{
    [System.Serializable]
    public class MultiplayerOptions : ICloneable
    {
        [DisplayName("Nickname")]
        public string Nickname { get; set; } = "Player";

        [DisplayName("Color Red")]
        [UIRange(0f, 1f)]
        public float ColorR { get; set; } = 1f;

        [DisplayName("Color Green")]
        [UIRange(0f, 1f)]
        public float ColorG { get; set; } = 0.6846404f;

        [DisplayName("Color Blue")]
        [UIRange(0f, 1f)]
        public float ColorB { get; set; } = 0.24313718f;

        [DisplayName("Host Port")]
        [UIRange(1, ushort.MaxValue)]
        public ushort HostPort { get; set; } = 8469;

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
