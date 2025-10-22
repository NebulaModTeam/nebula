#region

using System;
using System.IO;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using NebulaAPI.Interfaces;
using NebulaModel.Logger;
using NebulaModel.Utils;
using NebulaNetwork;
using NebulaPatcher.Logger;
using NebulaPatcher.MonoBehaviours;
using NebulaPatcher.Patches.Dynamic;
using NebulaPatcher.Patches.Misc;
using NebulaWorld;
using NebulaWorld.SocialIntegration;
using UnityEngine;

#endregion

namespace NebulaPatcher;

[BepInPlugin(PluginInfo.PLUGIN_ID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("dsp.common-api.CommonAPI", BepInDependency.DependencyFlags.SoftDependency)]
public class NebulaPlugin : BaseUnityPlugin, IMultiplayerMod
{
    private void Awake()
    {
        Log.Init(new BepInExLogger(Logger));

        NebulaModel.Config.ModInfo = Info;
        NebulaModel.Config.LoadOptions();
        NebulaModel.Config.LoadCommandLineOptions();
        if (!NebulaModel.Config.CommandLineOptions.VerifyStartupRequirements())
        {
            Application.Quit();
        }
        Multiplayer.IsDedicated = NebulaModel.Config.CommandLineOptions.IsDedicatedServer;

        try
        {
            Initialize();
        }
        catch (Exception ex)
        {
            Log.Error("Unhandled exception occurred while initializing Nebula:", ex);
        }
    }

    private void Update()
    {
        if (GameMain.isRunning && UIRoot.instance.launchSplash.willdone)
        {
            DiscordManager.Update();
        }
    }

    public string Version => NebulaModel.Config.ModVersion;

    public bool CheckVersion(string hostVersion, string clientVersion)
    {
        return hostVersion.Equals(clientVersion);
    }

    private static void Initialize()
    {
        InitPatches();
        AddNebulaBootstrapper();
        DiscordManager.Setup(ActivityManager_OnActivityJoin);
    }

    public static void StartDedicatedServer(string saveName)
    {
        // Mimic UI buttons clicking
        UIMainMenu_Patch.OnMultiplayerButtonClick();
        if (!GameSave.SaveExist(saveName))
        {
            return;
        }
        // Modified from DoLoadSelectedGame
        Log.Info($"Starting dedicated server, loading save : {saveName}");
        DSPGame.StartGame(saveName);
        Log.Info($"Listening server on port {NebulaModel.Config.Options.HostPort}");
        Multiplayer.HostGame(new Server(NebulaModel.Config.Options.HostPort, true));
        FPSController.SetFixUPS(NebulaModel.Config.CommandLineOptions.UpsValue);
    }

    public static void StartDedicatedServer(GameDesc gameDesc)
    {
        // Mimic UI buttons clicking
        UIMainMenu_Patch.OnMultiplayerButtonClick();
        if (gameDesc == null)
        {
            return;
        }
        // Modified from DoLoadSelectedGame
        Log.Info("Starting dedicated server, create new game from parameters:");
        Log.Info(
            $"seed={gameDesc.galaxySeed} starCount={gameDesc.starCount} resourceMultiplier={gameDesc.resourceMultiplier:F1}");
        DSPGame.StartGameSkipPrologue(gameDesc);
        Log.Info($"Listening server on port {NebulaModel.Config.Options.HostPort}");
        Multiplayer.HostGame(new Server(NebulaModel.Config.Options.HostPort, true));
        FPSController.SetFixUPS(NebulaModel.Config.CommandLineOptions.UpsValue);
    }

    private static async void ActivityManager_OnActivityJoin(string secret)
    {
        if (Multiplayer.IsActive)
        {
            Log.Warn("Cannot join lobby from Discord, we are already in a lobby.");
            return;
        }

        if (string.IsNullOrWhiteSpace(secret))
        {
            Log.Warn("Received Discord invite without IP address.");
            return;
        }

        var ipAddresses = secret.FromBase64().Split(';');

        if (ipAddresses.Length != 1 && ipAddresses.Length != 3)
        {
            Log.Warn("Received invalid discord invite.");
            return;
        }

        var ipAddress = string.Empty;

        if (ipAddresses.Length == 1 && ipAddresses[0].Contains("ngrok"))
        {
            ipAddress = ipAddresses[0];
        }

        if (string.IsNullOrWhiteSpace(ipAddress) && await IPUtils.IsIPv6Supported())
        {
            if (ipAddresses.Length > 1)
            {
                if (IPUtils.IsIPv6(ipAddresses[1]))
                {
                    ipAddress = $"{ipAddresses[1]}:{ipAddresses[2]}";
                }
            }
        }

        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            if (IPUtils.IsIPv4(ipAddresses[0]))
            {
                ipAddress = $"{ipAddresses[0]}:{ipAddresses[2]}";
            }
        }

        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            Log.Warn("Received Discord invite with invalid IP address.");
            return;
        }

        Log.Info("Joining lobby from Discord...");
        UIMainMenu_Patch.OnMultiplayerButtonClick();
        UIMainMenu_Patch.JoinGame($"{ipAddress}");
        DiscordManager.UpdateRichPresence(secret, secretPassthrough: true, updateTimestamp: true);
    }

    private static void InitPatches()
    {
        try
        {
            Log.Info("Patching Dyson Sphere Program...");
            Log.Info($"Applying patches from {PluginInfo.PLUGIN_NAME} {PluginInfo.PLUGIN_DISPLAY_VERSION} made for game version {DSPGameVersion.VERSION}");
            var timer = new HighStopwatch();
            timer.Begin();
#if DEBUG
            if (Directory.Exists("./mmdump"))
            {
                foreach (var file in new DirectoryInfo("./mmdump").GetFiles())
                {
                    file.Delete();
                }

                Environment.SetEnvironmentVariable("MONOMOD_DMD_TYPE", "cecil");
                Environment.SetEnvironmentVariable("MONOMOD_DMD_DUMP", "./mmdump");
            }
#endif
            var harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginInfo.PLUGIN_ID);
            harmony.PatchAll(typeof(Fix_Patches));
            if (Multiplayer.IsDedicated)
            {
                Log.Info("Patching for headless mode...");
                harmony.PatchAll(typeof(Dedicated_Server_Patches));
            }
#if DEBUG
            Environment.SetEnvironmentVariable("MONOMOD_DMD_DUMP", "");
#endif

            Log.Info("Patching completed successfully. Time cost: " + timer.duration);
        }
        catch (Exception ex)
        {
            Log.Error("Unhandled exception occurred while patching the game:", ex);
            // Show error in UIFatalErrorTip to inform normal users
            Harmony.CreateAndPatchAll(typeof(UIFatalErrorTip_Patch));
            Log.Error($"Nebula Multiplayer Mod is incompatible with game version, expected version {DSPGameVersion.VERSION}\nUnhandled exception occurred while patching the game.");
        }
    }

    private static void AddNebulaBootstrapper()
    {
        Log.Info("Applying Nebula Behaviors..");

        var nebulaRoot = new GameObject { name = "Nebula Multiplayer Mod" };
        nebulaRoot.AddComponent<NebulaBootstrapper>();

        Log.Info("Behaviors applied.");
    }
}
