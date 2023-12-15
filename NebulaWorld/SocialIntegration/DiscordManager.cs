#region

using System;
using System.IO;
using System.Text;
using Discord;
using NebulaModel;
using NebulaModel.Logger;
using NebulaModel.Utils;

#endregion

namespace NebulaWorld.SocialIntegration;

public static class DiscordManager
{
    private static Discord.Discord client;

    private static ActivityManager ActivityManager;

    private static Activity activity;

    private static readonly Random random = new();

    private static int SecretLength => 128;

    public static void Setup(ActivityManager.ActivityJoinHandler activityJoinHandler)
    {
        if (!Config.Options.EnableDiscordRPC)
        {
            Log.Info("Discord RPC support not enabled.");
            return;
        }

        try
        {
            client = new Discord.Discord(968766006182961182L, (ulong)CreateFlags.NoRequireDiscord);
        }
        catch (ResultException e)
        {
            Log.Warn(e);
            Cleanup();
            return;
        }

        ActivityManager = client.GetActivityManager();
        ActivityManager.RegisterCommand(Environment.CommandLine);

        var gameDir = new FileInfo(Environment.GetCommandLineArgs()[0]);
        if (gameDir.DirectoryName != null)
        {
            var steamAppIdFile = Path.Combine(gameDir.DirectoryName, "steam_appid.txt");
            if (!File.Exists(steamAppIdFile))
            {
                File.WriteAllText(steamAppIdFile, "1366540");
            }
        }

        Log.Info("Initialized Discord RPC");
        activity = new Activity
        {
            State = "In Menus",
            Timestamps = { Start = DateTimeOffset.Now.ToUnixTimeSeconds() },
            Party = { Id = CreateSecret(), Size = new PartySize() },
            Instance = true
        };
        UpdateActivity();

        ActivityManager.OnActivityJoinRequest += ActivityManager_OnActivityJoinRequest;
        ActivityManager.OnActivityJoin += activityJoinHandler;
    }

    private static void ActivityManager_OnActivityJoinRequest(ref User user)
    {
        // Print anonymized username of user who requested to join
        Log.Info($"Received Discord join request from user " +
                 $"{user.Username[0] + new string('*', user.Username.Length - 2) + user.Username[user.Username.Length - 1]}");

        if (!Config.Options.AutoAcceptDiscordJoinRequests)
        {
            return;
        }

        ActivityManager.SendRequestReply(user.Id, ActivityJoinRequestReply.Yes, result =>
        {
            Log.Info(result == Result.Ok ? "Accepted request." : "Could not accept request.");
        });
    }

    public static void Update()
    {
        if (!Config.Options.EnableDiscordRPC || client == null)
        {
            return;
        }
        try
        {
            client.RunCallbacks();
        }
        catch (ResultException e) // RunCallbacks throws an exception when Discord is not running.
        {
            Log.Warn(e);
            Cleanup();
        }
    }

    private static void UpdateActivity()
    {
        ActivityManager.UpdateActivity(activity, result =>
        {
            if (result == Result.Ok)
            {
                Log.Info("Updated Discord activity");
            }
            else
            {
                Log.Warn("Could not update Discord activity");
            }
        });
    }

    public static string CreateSecret()
    {
        var bytes = new byte[SecretLength];
        random.NextBytes(bytes);
        return Encoding.UTF8.GetString(bytes);
    }

    public static void Cleanup()
    {
        if (client == null)
        {
            return;
        }
        client.Dispose();
        client = null;
        Log.Info("Disposed Discord RPC");
    }

    public static void UpdateRichPresence(string ip = null, string partyId = null, bool secretPassthrough = false,
        bool updateTimestamp = false)
    {
        if (!Config.Options.EnableDiscordRPC || client == null)
        {
            return;
        }

        if (Multiplayer.IsActive)
        {
            activity.Party.Size.CurrentSize = Multiplayer.Session.NumPlayers;
            activity.Party.Size.MaxSize = ushort.MaxValue;
        }
        else
        {
            activity.Party.Size = new PartySize();
        }

        if (updateTimestamp)
        {
            activity.Timestamps.Start = DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        activity.State = Multiplayer.IsActive ? Multiplayer.Session.IsInLobby ? "In Lobby" : "In Game" : "In Menus";

        if (ip != null)
        {
            if (!string.IsNullOrWhiteSpace(ip) && !secretPassthrough)
            {
                ip = ip.ToBase64();
            }
            activity.Secrets.Join = ip;
        }

        if (!string.IsNullOrWhiteSpace(partyId))
        {
            activity.Party.Id = partyId;
        }

        UpdateActivity();
    }

    public static string GetPartyId()
    {
        if (!Config.Options.EnableDiscordRPC || client == null)
        {
            return null;
        }

        return activity.Party.Id;
    }
}
