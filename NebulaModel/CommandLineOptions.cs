#region

using System;
using System.IO;
using NebulaModel.Logger;

#endregion

namespace NebulaModel;

public class CommandLineOptions
{
    public bool IsDedicatedServer { get; set; }
    public bool IsBatchMode { get; set; }
    public string SaveName { get; set; } = string.Empty;
    public GameDesc NewGameDesc { get; set; }
    public float UpsValue { get; set; }
    public bool ShouldLoadGame => !string.IsNullOrEmpty(SaveName);
    public bool ShouldCreateNewGame => NewGameDesc != null;
    public bool LoadArgumentExists { get; set; }
    public bool NewGameArgumentExists { get; set; }


    public void ParseArgs(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-dsp_server":
                    IsDedicatedServer = true;
                    Log.Info(">> Initializing dedicated server");
                    break;

                case "-batchmode":
                    IsBatchMode = true;
                    break;

                case "-newgame":
                    ParseNewGameArguments(args, i);
                    break;

                case "-newgame-cfg":
                    ParseNewGameConfigArguments();
                    break;

                case "-load":
                    ParseLoadArgument(args, i);
                    break;

                case "-load-latest":
                    ParseLoadLatestArgument();
                    break;

                case "-ups":
                    ParseUpsArgument(args, i);
                    break;
            }
        }
    }

    public bool VerifyStartupRequirements()
    {
        if (IsDedicatedServer)
        {
            if (LoadArgumentExists && !ShouldLoadGame)
            {
                Log.Error(">> Can't find valid save to load! Exiting...");
                return false;
            }
            if (NewGameArgumentExists && !ShouldCreateNewGame)
            {
                Log.Error(">> New game parameters incorrect! Exiting...\nExpected parameters: -newgame seed starCount resourceMultiplier");
                return false;
            }
            if (!LoadArgumentExists && !NewGameArgumentExists)
            {
                Log.Error(">> -load or -newgame argument missing! Exiting...");
                return false;
            }
            if (!IsBatchMode)
            {
                Log.Warn(">> Dedicated server should be started with -batchmode argument");
            }
        }
        return true;
    }


    private bool ParseNewGameArguments(string[] args, int currentIndex)
    {
        NewGameArgumentExists = true;
        if (currentIndex + 3 < args.Length)
        {
            if (!int.TryParse(args[currentIndex + 1], out var seed))
            {
                Log.Warn($">> Can't set galaxy seed: {args[currentIndex + 1]} is not an integer");
            }
            else if (!int.TryParse(args[currentIndex + 2], out var starCount))
            {
                Log.Warn($">> Can't set star count: {args[currentIndex + 2]} is not an integer");
            }
            else if (!float.TryParse(args[currentIndex + 3], out var resourceMultiplier))
            {
                Log.Warn($">> Can't set resource multiplier: {args[currentIndex + 3]} is not a floating point number");
            }
            else
            {
                Log.Info($">> Creating new game ({seed}, {starCount}, {resourceMultiplier:F1})");
                var gameDesc = new GameDesc();
                gameDesc.SetForNewGame(UniverseGen.algoVersion, seed, starCount, 1, resourceMultiplier);
                NewGameDesc = gameDesc;
                return true;
            }
        }
        return false;
    }

    private bool ParseNewGameConfigArguments()
    {
        NewGameArgumentExists = true;

        // Create gamedesc with default values and assign a random seed
        var gameDesc = new GameDesc();
        var random = new DotNet35Random((int)(DateTime.UtcNow.Ticks / 10000L));
        gameDesc.SetForNewGame(UniverseGen.algoVersion, random.Next(100000000), 64, 1, 1f);

        // Overwrite the parameters in gamedesc from the config file (nebulaGameDescSettings.cfg)
        gameDesc = GameDescSettings.SetFromConfigFile(gameDesc);
        Log.Info($">> Creating new game ({gameDesc.galaxySeed}, {gameDesc.starCount}, {gameDesc.resourceMultiplier:F1})");
        NewGameDesc = gameDesc;
        return true;
    }

    private bool ParseLoadArgument(string[] args, int currentIndex)
    {
        LoadArgumentExists = true;
        if (currentIndex + 1 >= args.Length) return false;

        var saveName = args[currentIndex + 1];
        if (saveName.EndsWith(".dsv"))
        {
            saveName = saveName.Remove(saveName.Length - 4);
        }
        if (GameSave.SaveExist(saveName))
        {
            Log.Info($">> Loading save {saveName}");
            SaveName = saveName;
            return true;
        }
        else
        {
            Log.Error($">> Can't find save with name {saveName}!");
            return false;
        }
    }

    private bool ParseLoadLatestArgument()
    {
        LoadArgumentExists = true;
        string latestSaveName;
        try
        {
            latestSaveName = FindLatestSaveFile();
        }
        catch (Exception e)
        {
            Log.Error(">> Error when reading the save file folder: " + GameConfig.gameSaveFolder);
            Log.Error(e);
            return false;
        }
        if (string.IsNullOrEmpty(latestSaveName))
        {
            Log.Warn(">> Can't find save file in the save folder: " + GameConfig.gameSaveFolder);
            return false;
        }

        Log.Info($">> Loading latest save: {latestSaveName}");
        SaveName = latestSaveName;
        return true;
    }

    private static string FindLatestSaveFile()
    {
        var files = Directory.GetFiles(
            GameConfig.gameSaveFolder,
            "*" + GameSave.saveExt,
            SearchOption.TopDirectoryOnly
        );

        if (files.Length == 0)
        {
            return null;
        }
        var times = new long[files.Length];
        var names = new string[files.Length];
        for (var j = 0; j < files.Length; j++)
        {
            FileInfo fileInfo = new(files[j]);
            times[j] = fileInfo.LastWriteTime.ToFileTime();
            names[j] = fileInfo.Name.Substring(0, fileInfo.Name.Length - GameSave.saveExt.Length);
        }
        Array.Sort(times, names);
        return names[files.Length - 1];
    }

    private bool ParseUpsArgument(string[] args, int currentIndex)
    {
        if (currentIndex + 1 >= args.Length) return false;

        if (!float.TryParse(args[currentIndex + 1], out var value))
        {
            Log.Warn($">> Can't set UPS: {args[currentIndex + 1]} is not a valid number");
            return false;
        }

        Log.Info($">> Set UPS: {value}");
        UpsValue = value;
        return true;
    }
}
