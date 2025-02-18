#region

using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
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
using NebulaWorld.GameStates;
using NebulaWorld.SocialIntegration;
using UnityEngine;

#endregion

namespace NebulaPatcher;

[BepInPlugin(PluginInfo.PLUGIN_ID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("dsp.common-api.CommonAPI", BepInDependency.DependencyFlags.SoftDependency)]
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

            if (args[i] == "-newgame-cfg")
            {
                newgameArgExists = true;
                var gameDesc = new GameDesc();
                var random = new DotNet35Random((int)(DateTime.UtcNow.Ticks / 10000L));
                gameDesc.SetForNewGame(UniverseGen.algoVersion, random.Next(100000000), 64, 1, 1f);
                SetGameDescFromConfigFile(gameDesc);
                Log.Info($">> Creating new game ({gameDesc.galaxySeed}, {gameDesc.starCount}, {gameDesc.resourceMultiplier:F1})");
                GameStatesManager.NewGameDesc = gameDesc;
                didLoad = true;
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
                Log.Error(">> New game parameters incorrect! Exiting...\nExpect: -newgame seed starCount resourceMultiplier");
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

    public static void SetGameDescFromConfigFile(GameDesc gameDesc)
    {
        var customFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "nebulaGameDescSettings.cfg"), true);

        var galaxySeed = customFile.Bind("Basic", "galaxySeed", -1,
            "Cluster Seed. Negative value: Random or remain the same.").Value;
        if (galaxySeed >= 0)
        {
            gameDesc.galaxySeed = galaxySeed;
        }

        var starCount = customFile.Bind("Basic", "starCount", -1,
            "Number of Stars. Negative value: Default(64) or remain the same.").Value;
        if (starCount >= 0)
        {
            gameDesc.starCount = starCount;
        }

        var resourceMultiplier = customFile.Bind("Basic", "resourceMultiplier", -1f,
            "Resource Multiplier. Infinite = 100. Negative value: Default(1.0f) or remain the same.").Value;
        if (resourceMultiplier >= 0f)
        {
            gameDesc.resourceMultiplier = resourceMultiplier;
        }

        gameDesc.isPeaceMode = customFile.Bind("General", "isPeaceMode", false,
            "False: Enable enemy force (combat mode)").Value;
        gameDesc.isSandboxMode = customFile.Bind("General", "isSandboxMode", false,
            "True: Enable creative mode").Value;

        gameDesc.combatSettings.aggressiveness = customFile.Bind("Combat", "aggressiveness", 1f,
            new ConfigDescription("Aggressiveness (Dummy = -1, Rampage = 3)", new AcceptableValueList<float>(-1f, 0f, 0.5f, 1f, 2f, 3f))).Value;
        gameDesc.combatSettings.initialLevel = customFile.Bind("Combat", "initialLevel", 0,
            new ConfigDescription("Initial Level (Original range: 0 to 10)", new AcceptableValueRange<int>(0, 30))).Value;
        gameDesc.combatSettings.initialGrowth = customFile.Bind("Combat", "initialGrowth", 1f,
            "Initial Growth (Original range: 0 to 200%)").Value;
        gameDesc.combatSettings.initialColonize = customFile.Bind("Combat", "initialColonize", 1f,
            "Initial Occupation (Original range: 1% to 200%").Value;
        gameDesc.combatSettings.maxDensity = customFile.Bind("Combat", "maxDensity", 1f,
            "Max Density (Original range: 1 to 3)").Value;
        gameDesc.combatSettings.growthSpeedFactor = customFile.Bind("Combat", "growthSpeedFactor", 1f,
            "Growth Speed (Original range: 25% to 300%)").Value;
        gameDesc.combatSettings.powerThreatFactor = customFile.Bind("Combat", "powerThreatFactor", 1f,
            "Power Threat Factor (Original range: 1% to 1000%)").Value;
        gameDesc.combatSettings.battleThreatFactor = customFile.Bind("Combat", "battleThreatFactor", 1f,
            "Combat Threat Factor (Original range: 1% to 1000%)").Value;
        gameDesc.combatSettings.battleExpFactor = customFile.Bind("Combat", "battleExpFactor", 1f,
            "Combat XP Factor (Original range: 1% to 1000%)").Value;
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

            Log.Info("Patching completed successfully");
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
        Log.Info("Applying Nebula behaviours..");

        var nebulaRoot = new GameObject { name = "Nebula Multiplayer Mod" };
        nebulaRoot.AddComponent<NebulaBootstrapper>();

        Log.Info("Behaviours applied.");
    }
}
