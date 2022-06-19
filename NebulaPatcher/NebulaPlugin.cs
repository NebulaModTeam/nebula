using BepInEx;
using CommonAPI;
using CommonAPI.Systems;
using HarmonyLib;
using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Utils;
using NebulaNetwork;
using NebulaPatcher.Logger;
using NebulaPatcher.MonoBehaviours;
using NebulaPatcher.Patches.Dynamic;
using NebulaWorld;
using NebulaWorld.SocialIntegration;
using System;
using System.Net;
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
        static int command_ups = 0;

        private void Awake()
        {
            Log.Init(new BepInExLogger(Logger));

            NebulaModel.Config.ModInfo = Info;
            NebulaModel.Config.LoadOptions();

            // Read command-line arguments
            string[] args = Environment.GetCommandLineArgs();
            (bool didLoad, bool loadArgExists, string saveName) = (false, false, string.Empty);
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-server")
                {
                    Multiplayer.IsDedicated = true;
                    Log.Info($">> Initializing dedicated server");
                }

                if (args[i] == "-load" && i + 1 < args.Length)
                {
                    loadArgExists = true;
                    saveName = args[i + 1];
                    if (saveName.EndsWith(".dsv"))
                    {
                        saveName = saveName.Remove(saveName.Length - 4);
                    }
                    if (GameSave.SaveExist(saveName))
                    {
                        Log.Info($">> Loading save {saveName}");
                        NebulaWorld.GameStates.GameStatesManager.ImportedSaveName = saveName;
                        didLoad = true;
                    }
                }

                if (args[i] == "-ups" && i + 1 < args.Length)
                {
                    if (int.TryParse(args[i + 1], out int value))
                    {
                        Log.Info($">> Set UPS {value}");
                        command_ups = value;
                    }
                    else
                    {
                        Log.Warn($">> Can't set UPS, {args[i + 1]} is not a valid number");
                    }
                }
            }

            if (Multiplayer.IsDedicated && !didLoad)
            {
                if (loadArgExists)
                {
                    Log.Error($">> Can't find save with name {saveName}! Exiting...");
                }
                else
                {
                    Log.Error(">> -load argument missing! Exiting...");
                }
                Application.Quit();
            }

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
            DiscordManager.Setup(ActivityManager_OnActivityJoin);
        }

        public static void StartDedicatedServer(string saveName)
        {
            // Mimic UI buttons clicking
            UIMainMenu_Patch.OnMultiplayerButtonClick();            
            if (GameSave.SaveExist(saveName))
            {
                // Modified from DoLoadSelectedGame
                Log.Info($"Starting dedicated server, loading save : {saveName}");
                DSPGame.StartGame(saveName);
                Log.Info($"Listening server on port {NebulaModel.Config.Options.HostPort}");
                Multiplayer.HostGame(new Server(NebulaModel.Config.Options.HostPort, true));
                if (command_ups != 0)
                {
                    FPSController.SetFixUPS(command_ups);
                }
            }
        }

        private static async void ActivityManager_OnActivityJoin(string secret)
        {
            if(Multiplayer.IsActive)
            {
                Log.Warn("Cannot join lobby from Discord, we are already in a lobby.");
                return;
            }

            if(string.IsNullOrWhiteSpace(secret))
            {
                Log.Warn("Received Discord invite without IP address.");
                return;
            }

            var ipAddresses = secret.FromBase64().Split(';');

            if(ipAddresses.Length != 1 && ipAddresses.Length != 3)
            {
                Log.Warn("Received invalid discord invite.");
                return;
            }

            string ipAddress = string.Empty;

            if(ipAddresses.Length == 1 && ipAddresses[0].Contains("ngrok"))
            {
                ipAddress = ipAddresses[0];
            }

            if(string.IsNullOrWhiteSpace(ipAddress) && await IPUtils.IsIPv6Supported())
            {
                if(ipAddresses.Length > 1)
                {
                    if (IPUtils.IsIPv6(ipAddresses[1]))
                    {
                        ipAddress = $"{ipAddresses[1]}:{ipAddresses[2]}";
                    }
                }
            }

            if(string.IsNullOrWhiteSpace(ipAddress))
            {
                if (IPUtils.IsIPv4(ipAddresses[0]))
                {
                    ipAddress = $"{ipAddresses[0]}:{ipAddresses[2]}";
                }
            }

            if(string.IsNullOrWhiteSpace(ipAddress))
            {
                Log.Warn("Received Discord invite with invalid IP address.");
                return;
            }

            Log.Info("Joining lobby from Discord...");
            UIMainMenu_Patch.OnMultiplayerButtonClick();
            UIMainMenu_Patch.JoinGame($"{ipAddress}");
            DiscordManager.UpdateRichPresence(ip: secret, secretPassthrough: true, updateTimestamp: true);
        }

        private void Update()
        {
            if(GameMain.isRunning && UIRoot.instance.launchSplash.willdone)
            {
                DiscordManager.Update();
            }
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
                Harmony harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginInfo.PLUGIN_ID);
                if (Multiplayer.IsDedicated)
                {
                    Log.Info("Patching for headless mode...");
                    harmony.PatchAll(typeof(Dedicated_Server_Patch));
                }
#if DEBUG
                Environment.SetEnvironmentVariable("MONOMOD_DMD_DUMP", "");
#endif

                Log.Info("Patching completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error("Unhandled exception occurred while patching the game:", ex);
                // Show error in UIFatalErrorTip to inform normal users
                Harmony.CreateAndPatchAll(typeof(UIFatalErrorTip_Patch));
                Log.Error($"Nebula Multiplayer Mod is incompatible\nUnhandled exception occurred while patching the game.");
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
