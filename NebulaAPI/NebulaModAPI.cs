using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NebulaAPI
{
    [BepInPlugin(API_GUID, API_NAME, ThisAssembly.AssemblyFileVersion)]
    [BepInDependency(NEBULA_MODID, BepInDependency.DependencyFlags.SoftDependency)]
    public class NebulaModAPI : BaseUnityPlugin
    {
        private static bool _nebulaIsInstalled;
        private static Type _localPlayer;
        private static Type _factoryManager;
        private static Type _simulatedWorld;
        
        private static Type _binaryWriter;
        private static Type _binaryReader;
        
        public static readonly List<Assembly> TargetAssemblies = new List<Assembly>();
        public static readonly List<IModData<PlanetFactory>> FactorySerializers = new List<IModData<PlanetFactory>>();

        public const string NEBULA_MODID = "dsp.nebula-multiplayer";
        
        public const string API_GUID = "dsp.nebula-multiplayer-api";
        public const string API_NAME = "Nebula API";
        
        public static bool nebulaIsInstalled => _nebulaIsInstalled;

        private void Awake()
        {
            _nebulaIsInstalled = false;
                
            foreach (var pluginInfo in BepInEx.Bootstrap.Chainloader.PluginInfos)
            {
                if (pluginInfo.Value.Metadata.GUID != NEBULA_MODID) continue;

                _nebulaIsInstalled = true;
                break;
            }
            
            if (!_nebulaIsInstalled) return;

            _localPlayer = AccessTools.TypeByName("NebulaWorld.LocalPlayer");
            _factoryManager = AccessTools.TypeByName("NebulaWorld.Factory.FactoryManager");
            _simulatedWorld = AccessTools.TypeByName("NebulaWorld.SimulatedWorld");
            
            Type binaryUtils = AccessTools.TypeByName("NebulaModel.Networking.BinaryUtils");

            _binaryWriter = binaryUtils.GetNestedType("Writer");
            _binaryReader = binaryUtils.GetNestedType("Reader");
            
            Logger.LogInfo("Nebula API is ready!");
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