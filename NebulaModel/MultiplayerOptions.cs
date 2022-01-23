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

        [DisplayName("Show Lobby Hints")]
        public bool ShowLobbyHints { get; set; } = true;

        public string LastIP { get; set; } = string.Empty;

        public byte[] MechaAppearance { get; set; } = { };

        public MechaAppearance GetMechaAppearance()
        {
            MechaAppearance appearance = new MechaAppearance();
            if (MechaAppearance.Length > 0)
            {
                appearance.FromByte(MechaAppearance);
            }
            else
            {
                Logger.Log.Error($"Appearance is invalid.");
            }
            return appearance;
        }

        public void SetMechaAppearance()
        {
            MechaAppearance appearance = GameMain.mainPlayer.mecha.diyAppearance ?? GameMain.mainPlayer.mecha.appearance;
            MechaAppearance = appearance.ToByte();
            Config.SaveOptions();
        }

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
