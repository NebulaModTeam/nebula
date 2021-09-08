using NebulaAPI;
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

        [DisplayName("Host Port")]
        [UIRange(1, ushort.MaxValue)]
        public ushort HostPort { get; set; } = 8469;

        [DisplayName("Remember Last IP")]
        public bool RememberLastIP { get; set; } = true;

        public string LastIP { get; set; } = string.Empty;

        public string MechaColors { get; set; } = "209 151 76 255;184 90 72 255;94 92 92 255;123 234 255 255;229 155 94 255;255 243 235 255;255 248 245 255;255 255 255 255;";

        public Float4[] GetMechaColors()
        {
            string[] colors = MechaColors.Split(';');
            Float4[] mechaColors = new Float4[colors.Length - 1];
            for (int i = 0; i < colors.Length - 1; i++)
            {
                string[] color = colors[i].Split(' ');
                if (!float.TryParse(color[0], out mechaColors[i].x) ||
                    !float.TryParse(color[1], out mechaColors[i].y) ||
                    !float.TryParse(color[2], out mechaColors[i].z) ||
                    !float.TryParse(color[3], out mechaColors[i].w))
                {
                    Logger.Log.Error($"Color {i} is invalid.");
                }
            }
            return mechaColors;
        }

        public void SetMechaColors()
        {
            UnityEngine.Color32[] mainColors = GameMain.mainPlayer.mecha.mainColors;
            string mechaColors = string.Empty;
            for (int i = 0; i < mainColors.Length; i++)
            {
                mechaColors += $"{(int)mainColors[i].r} {(int)mainColors[i].g} {(int)mainColors[i].b} {(int)mainColors[i].a};";
            }
            MechaColors = mechaColors;
            Config.SaveOptions();
        }

        // Networking options
        [DisplayName("Connection Timeout (Seconds)")]
        public int Timeout { get; set; } = 30;

        [DisplayName("Max Packet Size (B/K/M/G)")]
        public string MaxMessageSize { get; set; } = "50M";

        public int GetMaxMessageSizeInBytes()
        {
            int msgSizeInBytes = 0;
            char endChar = char.ToUpper(MaxMessageSize[MaxMessageSize.Length - 1]);
            int multiplier;
            switch (endChar)
            {
                case 'G':
                    multiplier = 1024 * 1024 * 1024;
                    break;
                case 'M':
                    multiplier = 1024 * 1024;
                    break;
                case 'K':
                    multiplier = 1024;
                    break;
                case 'B':
                default:
                    multiplier = 1;
                    break;
            }
            if (int.TryParse(MaxMessageSize.TrimEnd(endChar), out int result))
            {
                msgSizeInBytes = result * multiplier;
            }
            return msgSizeInBytes;
        }

        [DisplayName("Queue Limit")]
        public int QueueLimit { get; set; } = 10000;

        [DisplayName("Packets per Tick")]
        public int PacketsPerTick { get; set; } = 1000;

        // Detail function group buttons
        public bool PowerGridEnabled { get; set; } = false;
        public bool VeinDistributionEnabled { get; set; } = false;
        public bool SpaceNavigationEnabled { get; set; } = true;
        public bool BuildingWarningEnabled { get; set; } = true;
        public bool BuildingIconEnabled { get; set; } = true;
        public bool GuidingLightEnabled { get; set; } = true;

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
