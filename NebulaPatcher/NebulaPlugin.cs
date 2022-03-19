﻿using BepInEx;
using CommonAPI;
using CommonAPI.Systems;
using HarmonyLib;
using NebulaAPI;
using NebulaModel.Logger;
using NebulaPatcher.Logger;
using NebulaPatcher.MonoBehaviours;
using System;
#if DEBUG
using System.IO;
#endif
using System.Reflection;
using UnityEngine;

namespace NebulaPatcher
{
    [BepInPlugin(PluginInfo.PLUGIN_ID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("DSPGAME.exe")]
    [BepInDependency(CommonAPIPlugin.GUID)]
    [CommonAPISubmoduleDependency(nameof(ProtoRegistry), nameof(CustomKeyBindSystem))]
    public class NebulaPlugin : BaseUnityPlugin, IMultiplayerMod
    {
        private void Awake()
        {
            Log.Init(new BepInExLogger(Logger));

            NebulaModel.Config.ModInfo = Info;
            NebulaModel.Config.LoadOptions();

            try
            {
                Initialize();
            }
            catch (Exception ex)
            {
                Log.Error("Unhandled exception occurred while initializing Nebula:", ex);
            }
        }

        private static void Initialize()
        {
            InitPatches();
            AddNebulaBootstrapper();
            RegisterKeyBinds();
        }

        private static void RegisterKeyBinds()
        {
            CustomKeyBindSystem.RegisterKeyBind<PressKeyBind>(new BuiltinKey
            {
                id = 212,
                key = new CombineKey((int)KeyCode.BackQuote, CombineKey.ALT_COMB, ECombineKeyAction.OnceClick, false),
                conflictGroup = 2052,
                name = "NebulaChatWindow",
                canOverride = true
            });
            ProtoRegistry.RegisterString("KEYNebulaChatWindow", "Show or Hide Chat Window");
        }

        private static void InitPatches()
        {
            Log.Info("Patching Dyson Sphere Program...");

            try
            {
                Log.Info($"Applying patches from {PluginInfo.PLUGIN_NAME} {PluginInfo.PLUGIN_DISPLAY_VERSION}");
#if DEBUG
                if (Directory.Exists("./mmdump"))
                {
                    foreach (FileInfo file in new DirectoryInfo("./mmdump").GetFiles())
                    {
                        file.Delete();
                    }

                    Environment.SetEnvironmentVariable("MONOMOD_DMD_TYPE", "cecil");
                    Environment.SetEnvironmentVariable("MONOMOD_DMD_DUMP", "./mmdump");
                }
#endif
                Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginInfo.PLUGIN_ID);
#if DEBUG
                Environment.SetEnvironmentVariable("MONOMOD_DMD_DUMP", "");
#endif

                Log.Info("Patching completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error("Unhandled exception occurred while patching the game:", ex);
            }
        }

        private static void AddNebulaBootstrapper()
        {
            Log.Info("Applying Nebula behaviours..");

            GameObject nebulaRoot = new GameObject
            {
                name = "Nebula Multiplayer Mod"
            };
            nebulaRoot.AddComponent<NebulaBootstrapper>();

            Log.Info("Behaviours applied.");
        }

        public string Version => NebulaModel.Config.ModVersion;
        public bool CheckVersion(string hostVersion, string clientVersion)
        {
            return hostVersion.Equals(clientVersion);
        }
    }
}
