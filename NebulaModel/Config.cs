using BepInEx;

namespace NebulaModel
{
    public static class Config
    {
        public static PluginInfo ModInfo { get; set; }
        public static string ModVersion => ModInfo?.Metadata.Version.ToString();
        public static int DefaultPort => 8469;


    }
}
