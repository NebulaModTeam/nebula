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

        [DisplayName("Remember Last IP")]
        public bool RememberLastIP { get; set; } = true;

        [DisplayName("Transport Layer")]
        public string TransportLayer { get; set; } = "telepathy";

        [DisplayName("Epic Online Services [Requires Restart]")]
        public bool EOSEnabled { get; set; } = true;

        [DisplayName("Connection Timeout (Seconds)")]
        public int Timeout { get; set; } = 30;

        [DisplayName("Max Packet Size (MB)")]
        public int MaxMessageSize { get; set; } = 50;

#if DEBUG
        [DisplayName("Simulate Latency")]
        public bool SimulateLatency { get; set; } = true;

        [DisplayName("Reliable Latency (Seconds)")]
        public float ReliableLatency { get; set; } = 2f;

        [DisplayName("Unreliable Loss (%)")]
        [UIRange(0, 1)]
        public float UnreliableLoss { get; set; } = 0.5f;

        [DisplayName("Unreliable Latency (Seconds)")]
        public float UnreliableLatency { get; set; } = 2f;

        [DisplayName("Unreliable Scramble (%)")]
        [UIRange(0, 1)]
        public float UnreliableScramble { get; set; } = 1f;
#endif

        public string LastIP { get; set; } = string.Empty;

        // Detail function group buttons
        public bool PowerGridEnabled { get; set; } = false;
        public bool VeinDistributionEnabled { get; set; } = false;
        public bool SpaceNavigationEnabled { get; set; } = true;
        public bool BuildingWarningEnabled { get; set; } = true;
        public bool BuildingIconEnabled { get; set; } = true;
        public bool GuidingLightEnabled { get; set; } = true;

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
