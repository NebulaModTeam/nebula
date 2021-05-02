using BepInEx;
using NebulaModel.Logger;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace NebulaModel
{
    public static class Config
    {
        private const string OPTION_SAVE_FILE = "nebula.cfg";

        public static PluginInfo ModInfo { get; set; }
        public static string ModVersion => ThisAssembly.AssemblyInformationalVersion;
        public static MultiplayerOptions Options { get; set; }

        public static bool LoadOptions()
        {
            Options = new MultiplayerOptions();

            string path = Path.Combine(GameConfig.gameDocumentFolder, OPTION_SAVE_FILE);
            if (File.Exists(path))
            {
                try
                {
                    var formatter = new BinaryFormatter();
                    using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        Options = (MultiplayerOptions)formatter.Deserialize(stream);
                    }
                }
                catch(System.Exception e)
                {
                    Log.Error($"Could not load {OPTION_SAVE_FILE}", e);
                    return false;
                }
            }
            
            return true;
        }

        public static bool SaveOptions()
        {
            if (Options == null)
            {
                Log.Error($"Cannot save {OPTION_SAVE_FILE}, is null");
                return false;
            }

            string path = Path.Combine(GameConfig.gameDocumentFolder, OPTION_SAVE_FILE);
            try
            {
                var formatter = new BinaryFormatter();
                using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    formatter.Serialize(stream, Options);
                }
                return true;
            }
            catch (System.Exception e)
            {
                Log.Error($"Could not load {OPTION_SAVE_FILE}", e);
                return false;
            }
        }
    }
}
