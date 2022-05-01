using Discord;
using NebulaWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NebulaNetwork
{
    public class DiscordManager
    {
        private static Discord.Discord client;
        public static ActivityManager ActivityManager { get; private set; }

        private static Activity activity;

        private static readonly Random random = new();

        private static int SecretLength => 128;

        public static void Setup()
        {
            if(!NebulaModel.Config.Options.EnableDiscordRPC)
            {
                NebulaModel.Logger.Log.Info("Discord RPC support not enabled.");
                return;
            }

            client = new(968766006182961182L, (ulong)CreateFlags.Default);
            ActivityManager = client.GetActivityManager();
            ActivityManager.RegisterCommand(Environment.CommandLine);

            var gameDir = new FileInfo(Environment.GetCommandLineArgs()[0]);
            var steamAppIdFile = Path.Combine(gameDir.DirectoryName, "steam_appid.txt");
            if(!File.Exists(steamAppIdFile))
            {
                File.WriteAllText(steamAppIdFile, "1366540");
            }

            NebulaModel.Logger.Log.Info("Initialized Discord RPC");
            activity = new()
            {
                Details = "Playing Nebula Multiplayer Mod",
                State = "In Main Menu",
                Timestamps =
                {
                    Start = 0
                },
                Party =
                {
                    Id = CreateSecret(),
                    Size =
                    {
                        CurrentSize = 1,
                        MaxSize = 1
                    }
                },
                Instance = true,
            };
            UpdateActivity();

            ActivityManager.OnActivityJoinRequest += ActivityManager_OnActivityJoinRequest;
        }

        private static void ActivityManager_OnActivityJoinRequest(ref User user)
        {
            NebulaModel.Logger.Log.Info($"Received Discord join request from user {user.Username}");

            if(!NebulaModel.Config.Options.AutoAcceptDiscordJoinRequests)
            {
                return;
            }

            ActivityManager.SendRequestReply(user.Id, ActivityJoinRequestReply.Yes, result =>
            {
                if(result == Result.Ok)
                {
                    NebulaModel.Logger.Log.Info("Accepted request.");
                }
                else
                {
                    NebulaModel.Logger.Log.Info("Could not accept request.");
                }
            });
        }

        public static void Update()
        {
            if(NebulaModel.Config.Options.EnableDiscordRPC)
            {
                client.RunCallbacks();
            }
        }

        private static void UpdateActivity()
        {
            ActivityManager.UpdateActivity(activity, (result) => 
            { 
                if(result == Result.Ok)
                {
                    NebulaModel.Logger.Log.Info("Updated Discord activity");
                }
                else
                {
                    NebulaModel.Logger.Log.Warn("Could not update Discord activity");
                }
            });
        }

        private static string CreateSecret()
        {
            byte[] bytes = new byte[SecretLength];
            random.NextBytes(bytes);
            return Encoding.UTF8.GetString(bytes);
        }

        public static void Cleanup()
        {
            client.Dispose();
            NebulaModel.Logger.Log.Info("Disposed Discord RPC");
        }

        public static void UpdateRichPresence(string ip = null)
        {
            if(!Multiplayer.IsActive || !NebulaModel.Config.Options.EnableDiscordRPC)
            {
                return;
            }

            var numPlayers = Multiplayer.Session.Network.PlayerManager.GetAllPlayerDataIncludingHost().Length;
            activity.Party.Size.CurrentSize = numPlayers;
            activity.Party.Size.MaxSize = int.MaxValue;
            activity.State = $"Building the factory with {numPlayers - 1} others";

            if(!string.IsNullOrWhiteSpace(ip))
            {
                activity.Secrets.Join = ip;
            }

            UpdateActivity();
        }
    }
}
