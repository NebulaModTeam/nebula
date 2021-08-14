using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NebulaAPI
{
    public static class NebulaModAPI
    {
        private static bool _nebulaIsInstalled;
        private static bool _initialized;
        
        public static readonly List<Assembly> TargetAssemblies = new List<Assembly>();
        
        public const string NebulaModid = "dsp.nebula-multiplayer";

        public static bool nebulaIsInstalled
        {
            get
            {
                if (_initialized) return _nebulaIsInstalled;
                
                Init();
                return _nebulaIsInstalled;
            }
        }

        private static void Init()
        {
            if (_initialized) return;
            
            _nebulaIsInstalled = false;
                
            foreach (var pluginInfo in BepInEx.Bootstrap.Chainloader.PluginInfos)
            {
                if (pluginInfo.Value.Metadata.GUID != NebulaModid) continue;

                _nebulaIsInstalled = true;
                break;
            }
            if (!_nebulaIsInstalled) return;

            LocalPlayer.Init();
            FactoryManager.Init();
                
            /*Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if (!assembly.FullName.StartsWith("NebulaWorld")) continue;
                
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.FullName == "NebulaWorld.LocalPlayer")
                    {
                        _localPlayer = type;
                    }
                }
            }*/

            _initialized = true;
        }

        public static void RegisterPackets(Assembly assembly)
        {
            TargetAssemblies.Add(assembly);
        }
    }
}