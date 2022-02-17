using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using NebulaModel.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NebulaModel
{
    public static class Config
    {
        private const string OPTION_SAVE_FILE = "nebula.cfg";
        private const string SECTION_NAME = "Nebula - Settings";

        public static PluginInfo ModInfo { get; set; }
        public static string ModVersion => ThisAssembly.AssemblyInformationalVersion;
        public static MultiplayerOptions Options { get; set; }

        public static Action OnConfigApplied;
        
        private static ConfigFile configFile;
        

        public static bool LoadOptions()
        {
            Options = new MultiplayerOptions();

            try
            {
                configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, OPTION_SAVE_FILE), true);

                List<PropertyInfo> properties = AccessTools.GetDeclaredProperties(typeof(MultiplayerOptions));
                MethodInfo configBindMethod = typeof(ConfigFile)
                    .GetMethods()
                    .Where(m => m.Name == nameof(ConfigFile.Bind))
                    .First(m => m.IsGenericMethod && m.GetParameters().Length == 4);

                foreach (PropertyInfo prop in properties)
                {
                    object entry = configBindMethod.MakeGenericMethod(prop.PropertyType).Invoke(configFile,
                        new object[] { SECTION_NAME, prop.Name, prop.GetValue(Options), null });

                    Type entryType = typeof(ConfigEntry<>).MakeGenericType(prop.PropertyType);
                    prop.SetValue(Options, AccessTools.Property(entryType, "Value").GetValue(entry));
                }
            }
            catch (Exception e)
            {
                Log.Error($"Could not load {OPTION_SAVE_FILE}", e);
                return false;
            }

            return true;
        }

        public static bool SaveOptions()
        {
            try
            {
                List<PropertyInfo> properties = AccessTools.GetDeclaredProperties(typeof(MultiplayerOptions));
                foreach (PropertyInfo prop in properties)
                {
                    ConfigDefinition key = new ConfigDefinition(SECTION_NAME, prop.Name);
                    configFile[key].BoxedValue = prop.GetValue(Options);
                }
                configFile.Save();
            }
            catch (Exception e)
            {
                Log.Error($"Could not load {OPTION_SAVE_FILE}", e);
                return false;
            }

            return true;
        }
    }
}
