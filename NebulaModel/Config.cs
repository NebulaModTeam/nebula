using BepInEx;

namespace NebulaModel
{
    public static class Config
    {
        public static PluginInfo ModInfo { get; set; }
        public static System.Version ModVersion => ModInfo.Metadata.Version;
        public static int DefaultPort => 8469;
    }
}
