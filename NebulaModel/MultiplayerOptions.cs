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

        public string MechaAppearance { get; set; } = "";

        public MechaAppearance GetMechaAppearance()
        {
            if (MechaAppearance.Length > 0)
            {
                var defaultAppearance = new MechaAppearance();
                defaultAppearance.Init();

                defaultAppearance.FromByte(GetAppearanceAsByteArray());
                return defaultAppearance;
            }
            else
            {
                Logger.Log.Error("Appearance is invalid. Fallback to mainPlayer");
                //not sure if we should fallback
                return GameMain.mainPlayer.mecha.appearance;
            }
        }

        public void SetMechaAppearance()
        {
            string appearance = GetAppearanceAsByteString();
            MechaAppearance = appearance;
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

        private byte[] GetAppearanceAsByteArray()
        {
            var a = MechaAppearance.Split(';');

            var b = new byte[a.Length - 1];
            for (var i = 0; i < a.Length - 1; i++)
            {
                b[i] = byte.Parse(a[i]);
            }

            return b;
        }

        private string GetAppearanceAsByteString()
        {
            var appearance = GameMain.mainPlayer.mecha.diyAppearance;
            var stri = "";
            foreach (var b in appearance.ToByte())
            {
                stri += $"{b};";
            }

            return stri;
        }
    }
}
