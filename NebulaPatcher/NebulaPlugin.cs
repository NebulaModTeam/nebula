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
using NebulaWorld;
using NebulaWorld.GameStates;
using NebulaWorld.SocialIntegration;
using UnityEngine;

#endregion

namespace NebulaPatcher;

[BepInPlugin(PluginInfo.PLUGIN_ID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("dsp.common - api.CommonAPI", BepInDependency.DependencyFlags.SoftDependency)]
public class NebulaPlugin : BaseUnityPlugin, IMultiplayerMod
{
    private static int command_ups;

    private void Awake()
    {
        Log.Init(new BepInExLogger(Logger));

        NebulaModel.Config.ModInfo = Info;
        NebulaModel.Config.LoadOptions();

        // Read command-line arguments
        var args = Environment.GetCommandLineArgs();
        var batchmode = false;
        var (didLoad, loadArgExists, newgameArgExists, saveName) = (false, false, false, string.Empty);
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "-server")
            {
                Multiplayer.IsDedicated = true;
                Log.Info(">> Initializing dedicated server");
            }

            if (args[i] == "-batchmode")
            {
                batchmode = true;
            }

            if (args[i] == "-newgame")
            {
                newgameArgExists = true;
                if (i + 3 < args.Length)
                {
                    if (!int.TryParse(args[i + 1], out var seed))
                    {
                        Log.Warn($">> Can't set galaxy seed: {args[i + 1]} is not a integer");
                    }
                    else if (!int.TryParse(args[i + 2], out var starCount))
                    {
                        Log.Warn($">> Can't set star count: {args[i + 2]} is not a integer");
                    }
                    else if (!float.TryParse(args[i + 3], out var resourceMultiplier))
                    {
                        Log.Warn($">> Can't set resource multiplier: {args[i + 3]} is not a floating point number");
                    }
                    else
                    {
                        Log.Info($">> Creating new game ({seed}, {starCount}, {resourceMultiplier:F1})");
                        var gameDesc = new GameDesc();
                        gameDesc.SetForNewGame(UniverseGen.algoVersion, seed, starCount, 1, resourceMultiplier);
                        GameStatesManager.NewGameDesc = gameDesc;
                        didLoad = true;
                    }
                }
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
                    GameStatesManager.ImportedSaveName = saveName;
                    didLoad = true;
                }
            }

            if (args[i] == "-load-latest")
            {
                loadArgExists = true;
                var files = Directory.GetFiles(GameConfig.gameSaveFolder, "*" + GameSave.saveExt,
                    SearchOption.TopDirectoryOnly);
                var times = new long[files.Length];
                var names = new string[files.Length];
                for (var j = 0; j < files.Length; j++)
                {
                    FileInfo fileInfo = new(files[j]);
                    times[j] = fileInfo.LastWriteTime.ToFileTime();
                    names[j] = fileInfo.Name.Substring(0, fileInfo.Name.Length - GameSave.saveExt.Length);
                }
                if (files.Length > 0)
                {
                    Array.Sort(times, names);
                    saveName = names[files.Length - 1];
                    Log.Info($">> Loading save {saveName}");
                    GameStatesManager.ImportedSaveName = saveName;
                    didLoad = true;
                }
            }

            if (args[i] != "-ups" || i + 1 >= args.Length)
            {
                continue;
            }
            if (int.TryParse(args[i + 1], out var value))
            {
                Log.Info($">> Set UPS {value}");
                command_ups = value;
            }
            else
            {
                Log.Warn($">> Can't set UPS, {args[i + 1]} is not a valid number");
            }
        }

        if (Multiplayer.IsDedicated && !didLoad)
        {
            if (loadArgExists)
            {
                Log.Error(saveName != string.Empty
                    ? $">> Can't find save with name {saveName}! Exiting..."
                    : ">> Can't find any save in the folder! Exiting...");
            }
            else if (newgameArgExists)
            {
                Log.Error(">> New game parameters incorrect! Exiting...\nExpect: -newgame seed starCount resourceMltiplier");
            }
            else
            {
                Log.Error(">> -load or -newgame argument missing! Exiting...");
            }
            Application.Quit();
        }

        if (Multiplayer.IsDedicated)
        {
            if (!batchmode)
            {
                Log.Warn("Dedicated server should be started with -batchmode argument");
            }
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
        if (command_ups != 0)
        {
            FPSController.SetFixUPS(command_ups);
        }
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
        if (command_ups != 0)
        {
            FPSController.SetFixUPS(command_ups);
        }
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
        Log.Info("Patching Dyson Sphere Program...");

        try
        {
            Log.Info($"Applying patches from {PluginInfo.PLUGIN_NAME} {PluginInfo.PLUGIN_DISPLAY_VERSION}");
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
            Log.Error("Nebula Multiplayer Mod is incompatible with game version\nUnhandled exception occurred while patching the game.");
        }
    }

    private static void AddNebulaBootstrapper()
    {
        Log.Info("Applying Nebula behaviours..");

        var nebulaRoot = new GameObject { name = "Nebula Multiplayer Mod" };
        nebulaRoot.AddComponent<NebulaBootstrapper>();

        Log.Info("Behaviours applied.");
    }
}
