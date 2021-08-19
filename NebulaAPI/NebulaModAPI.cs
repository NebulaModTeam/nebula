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
        private static bool nebulaIsInstalled;
        private static Type localPlayer;
        private static Type factoryManager;
        private static Type simulatedWorld;
        
        private static Type binaryWriter;
        private static Type binaryReader;
        
        public static readonly List<Assembly> TargetAssemblies = new List<Assembly>();
        public static readonly List<IModData<PlanetFactory>> FactorySerializers = new List<IModData<PlanetFactory>>();

        public const string NEBULA_MODID = "dsp.nebula-multiplayer";
        
        public const string API_GUID = "dsp.nebula-multiplayer-api";
        public const string API_NAME = "Nebula API";
        
        public static bool NebulaIsInstalled => nebulaIsInstalled;

        private void Awake()
        {
            nebulaIsInstalled = false;
                
            foreach (var pluginInfo in BepInEx.Bootstrap.Chainloader.PluginInfos)
            {
                if (pluginInfo.Value.Metadata.GUID != NEBULA_MODID) continue;

                nebulaIsInstalled = true;
                break;
            }
            
            if (!nebulaIsInstalled) return;

            localPlayer = AccessTools.TypeByName("NebulaWorld.LocalPlayer");
            factoryManager = AccessTools.TypeByName("NebulaWorld.Factory.FactoryManager");
            simulatedWorld = AccessTools.TypeByName("NebulaWorld.SimulatedWorld");
            
            Type binaryUtils = AccessTools.TypeByName("NebulaModel.Networking.BinaryUtils");

            binaryWriter = binaryUtils.GetNestedType("Writer");
            binaryReader = binaryUtils.GetNestedType("Reader");
            
            Logger.LogInfo("Nebula API is ready!");
        }

        public const int PLANET_NONE = -2;
        public const int AUTHOR_NONE = -1;
        public const int STAR_NONE = -1;

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
            if (!NebulaIsInstalled) return null;
            
            return (INebulaPlayer) localPlayer.GetField("Instance").GetValue(null);
        }
        
        public static IFactoryManager GetFactoryManager()
        {
            if (!NebulaIsInstalled) return null;
            
            return (IFactoryManager) factoryManager.GetField("Instance").GetValue(null);
        }
        
        public static ISimulatedWorld GetSimulatedWorld()
        {
            if (!NebulaIsInstalled) return null;
            
            return (ISimulatedWorld) simulatedWorld.GetField("Instance").GetValue(null);
        }
        
        public static IWriterProvider GetBinaryWriter()
        {
            if (!NebulaIsInstalled) return null;

            return (IWriterProvider) binaryWriter.GetConstructor(new Type[0]).Invoke(new object[0]);
        }
        
        public static IReaderProvider GetBinaryReader(byte[] bytes)
        {
            if (!NebulaIsInstalled) return null;

            return (IReaderProvider) binaryReader.GetConstructor(new[]{typeof(byte[])}).Invoke(new object[]{bytes});
        }
    }
}