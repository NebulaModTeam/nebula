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
        
        private static Type multiplayer;
        
        private static Type binaryWriter;
        private static Type binaryReader;
        
        public static readonly List<Assembly> TargetAssemblies = new List<Assembly>();

        public const string NEBULA_MODID = "dsp.nebula-multiplayer";
        
        public const string API_GUID = "dsp.nebula-multiplayer-api";
        public const string API_NAME = "NebulaMultiplayerModApi";
        
        public static bool NebulaIsInstalled => nebulaIsInstalled;
        
        /// <summary>
        /// Is this session in multiplayer
        /// </summary>
        public static bool IsMultiplayerActive
        {
            get
            {
                if (!NebulaIsInstalled) return false;
                
                return (bool)multiplayer.GetProperty("IsActive").GetValue(null);
            }
        }
        
        /// <summary>
        /// Provides access to MultiplayerSession class
        /// </summary>
        public static IMultiplayerSession MultiplayerSession
        {
            get
            {
                if (!NebulaIsInstalled) return null;
                
                return (IMultiplayerSession) multiplayer.GetProperty("Session").GetValue(null);
            }
        }

        /// <summary>
        /// Subscribe to receive event when new multiplayer game is started
        /// </summary>
        public static Action OnMultiplayerGameStarted;
        
        /// <summary>
        /// Subscribe to receive event when multiplayer game end
        /// </summary>
        public static Action OnMultiplayerGameEnded;

        /// <summary>
        /// Subscribe to receive event when a new star starts loaded<br/>
        /// int starIndex - index of star to load
        /// </summary>
        public static Action<int> OnStarLoadRequest;
        
        /// <summary>
        /// Subscribe to receive event when a new planet starts loaded<br/>
        /// int planetId - id of planet to load
        /// </summary>
        public static Action<int> OnPlanetLoadRequest;
        
        /// <summary>
        /// Subscribe to receive event when a new planet is finished loading<br/>
        /// int planetId - id of planet to load
        /// </summary>
        public static Action<int> OnPlanetLoadFinished;
        
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

            multiplayer = AccessTools.TypeByName("NebulaWorld.Multiplayer");
            
            Type binaryUtils = AccessTools.TypeByName("NebulaModel.Networking.BinaryUtils");

            binaryWriter = binaryUtils.GetNestedType("Writer");
            binaryReader = binaryUtils.GetNestedType("Reader");
            
            Logger.LogInfo("Nebula API is ready!");
        }

        public const int PLANET_NONE = -2;
        public const int AUTHOR_NONE = -1;
        public const int STAR_NONE = -1;

        /// <summary>
        /// Register all packets within assembly
        /// </summary>
        /// <param name="assembly">Target assembly</param>
        public static void RegisterPackets(Assembly assembly)
        {
            TargetAssemblies.Add(assembly);
        }

        /// <summary>
        /// Provides access to BinaryWriter with LZ4 compression
        /// </summary>
        public static IWriterProvider GetBinaryWriter()
        {
            if (!NebulaIsInstalled) return null;

            return (IWriterProvider) binaryWriter.GetConstructor(new Type[0]).Invoke(new object[0]);
        }
        
        /// <summary>
        /// Provides access to BinaryReader with LZ4 compression
        /// </summary>
        public static IReaderProvider GetBinaryReader(byte[] bytes)
        {
            if (!NebulaIsInstalled) return null;

            return (IReaderProvider) binaryReader.GetConstructor(new[]{typeof(byte[])}).Invoke(new object[]{bytes});
        }
    }
}