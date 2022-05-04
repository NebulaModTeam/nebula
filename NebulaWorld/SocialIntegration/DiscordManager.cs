using Discord;
using NebulaWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NebulaWorld.SocialIntegration
{
    public class DiscordManager
    {
        private static Discord.Discord client;

        private static ActivityManager ActivityManager;

        private static Activity activity;

        private static readonly Random random = new();

        private static int SecretLength => 128;

        public static void Setup(ActivityManager.ActivityJoinHandler activityJoinHandler)
        {
            if(!NebulaModel.Config.Options.EnableDiscordRPC)
            {
                NebulaModel.Logger.Log.Info("Discord RPC support not enabled.");
                return;
            }

            try
            {
                client = new(968766006182961182L, (ulong)CreateFlags.NoRequireDiscord);
            }
            catch(ResultException e)
            {
                NebulaModel.Logger.Log.Warn(e);
                Cleanup();
                return;
            }

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
                State = "In Menus",
                Timestamps =
                {
                    Start = 0
                },
                Party =
                {
                    Id = CreateSecret(),
                    Size = new()
                },
                Instance = true,
            };
            UpdateActivity();

            ActivityManager.OnActivityJoinRequest += ActivityManager_OnActivityJoinRequest;
            ActivityManager.OnActivityJoin += activityJoinHandler;
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
            if(NebulaModel.Config.Options.EnableDiscordRPC && client != null)
            {
                try
                {
                    client.RunCallbacks();
                }
                catch (ResultException e) // RunCallbacks throws an exception when Discord is not running.
                {
                    NebulaModel.Logger.Log.Warn(e);
                    Cleanup();
                }
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
            if(client != null)
            {
                client.Dispose();
                client = null;
                NebulaModel.Logger.Log.Info("Disposed Discord RPC");
            }
        }

        public static void UpdateRichPresence(string ip = null)
        {
            if(!NebulaModel.Config.Options.EnableDiscordRPC || client == null)
            {
                return;
            }

            if(Multiplayer.IsActive)
            {
                activity.Party.Size.CurrentSize = Multiplayer.Session.NumPlayers;
                activity.Party.Size.MaxSize = ushort.MaxValue;
            }
            else
            {
                activity.Party.Size = new();
            }

            activity.State = Multiplayer.IsActive ? (Multiplayer.Session.IsInLobby ? "In Lobby" : "In Game") : "In Menus";

            if(ip != null)
            {
                activity.Secrets.Join = ip;
            }

            UpdateActivity();
        }
    }
}
