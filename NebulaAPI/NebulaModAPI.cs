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
        
        private static Type _localPlayer;
        private static Type _factoryManager;
        private static Type _simulatedWorld;
        
        private static Type _binaryWriter;
        private static Type _binaryReader;
        
        public static readonly List<Assembly> TargetAssemblies = new List<Assembly>();
        public static readonly List<IModData<PlanetFactory>> FactorySerializers = new List<IModData<PlanetFactory>>();
        
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
            _initialized = true;
            if (!_nebulaIsInstalled) return;

            _localPlayer = AccessTools.TypeByName("NebulaWorld.LocalPlayer");
            _factoryManager = AccessTools.TypeByName("NebulaWorld.Factory.FactoryManager");
            _simulatedWorld = AccessTools.TypeByName("NebulaWorld.SimulatedWorld");
            
            Type binaryUtils = AccessTools.TypeByName("NebulaModel.Networking.BinaryUtils");

            _binaryWriter = binaryUtils.GetNestedType("Writer");
            _binaryReader = binaryUtils.GetNestedType("Reader");
        }

        public static void RegisterPackets(Assembly assembly)
        {
            TargetAssemblies.Add(assembly);
        }
        
        public static void RegisterModFactoryData(IModData<PlanetFactory> serializer)
        {
            FactorySerializers.Add(serializer);
        }

        public static INebulaPlayer GetLocalPlayer()
        {
            if (!nebulaIsInstalled) return null;
            
            return (INebulaPlayer) _localPlayer.GetField("Instance").GetValue(null);
        }
        
        public static IFactoryManager GetFactoryManager()
        {
            if (!nebulaIsInstalled) return null;
            
            return (IFactoryManager) _factoryManager.GetField("Instance").GetValue(null);
        }
        
        public static ISimulatedWorld GetSimulatedWorld()
        {
            if (!nebulaIsInstalled) return null;
            
            return (ISimulatedWorld) _simulatedWorld.GetField("Instance").GetValue(null);
        }
        
        public static IWriterProvider GetBinaryWriter()
        {
            if (!nebulaIsInstalled) return null;

            return (IWriterProvider) _binaryWriter.GetConstructor(new Type[0]).Invoke(new object[0]);
        }
        
        public static IReaderProvider GetBinaryReader(byte[] bytes)
        {
            if (!nebulaIsInstalled) return null;

            return (IReaderProvider) _binaryReader.GetConstructor(new[]{typeof(byte[])}).Invoke(new object[]{bytes});
        }
    }
}