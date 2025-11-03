#region

using System;
using System.Threading.Tasks;
using BepInEx;
using NebulaModel;
using NebulaModel.DataStructures.Chat;
using NebulaWorld.MonoBehaviours.Local.Chat;
using NebulaWorld.Planet;

#endregion

namespace NebulaWorld.Chat.Commands;

public class DevCommandHandler : IChatCommandHandler
{
    public void Execute(ChatService chatService, string[] parameters)
    {
        if (parameters.Length < 1)
        {
            throw new ChatCommandUsageException("Require at least 1 argument!".Translate());
        }

        switch (parameters[0])
        {
            case "sandbox":
                {
                    GameMain.sandboxToolsEnabled = !GameMain.sandboxToolsEnabled;
                    GameMain.data.gameDesc.isSandboxMode = GameMain.sandboxToolsEnabled;
                    chatService.AddMessage("SandboxTool enable: " + GameMain.sandboxToolsEnabled, ChatMessageType.CommandOutputMessage);
                    return;
                }
            case "load-cfg":
                {
                    // Adjust combat settings or make resources infinite
                    chatService.AddMessage("Overwrite settings from nebulaGameDescSettings.cfg", ChatMessageType.CommandOutputMessage);
                    var gameDesc = GameMain.data.gameDesc;
                    var starCount = gameDesc.starCount;
                    var galaxySeed = gameDesc.galaxySeed;
                    GameMain.data.gameDesc.ApplyModConfigFileSettings();
                    // starCount and galaxySeed should not be changed after the game is created
                    gameDesc.starCount = starCount;
                    gameDesc.galaxySeed = galaxySeed;
                    return;
                }

            case "self-destruct":
                {
                    GameMain.mainPlayer.Kill();
                    return;
                }

            case "unload-factories":
                {
                    if (Multiplayer.Session.IsServer)
                    {
                        chatService.AddMessage("this command is only available for client", ChatMessageType.CommandOutputMessage);
                    }
                    else if (GameMain.localPlanet != null)
                    {
                        chatService.AddMessage("can only unload when in space", ChatMessageType.CommandOutputMessage);
                    }
                    else
                    {
                        var factoryCount = GameMain.data.factoryCount;
                        PlanetManager.UnloadAllFactories();
                        chatService.AddMessage($"unload factory count: {factoryCount}", ChatMessageType.CommandOutputMessage);
                    }
                    return;
                }

            case "ping":
                {
                    if (parameters.Length >= 2 && int.TryParse(parameters[1], out var value))
                    {
                        _ = DelayedResponse(value);
                        return;
                    }
                    chatService.AddMessage("Pong", ChatMessageType.CommandOutputMessage);
                }
                return;

            case "trigger-error":
                {
                    throw new NullReferenceException();
                }

            default:
                chatService.AddMessage("Unknown command: " + parameters[0], ChatMessageType.CommandOutputMessage);
                return;
        }
    }

    public string GetDescription()
    {
        return "Developer/Sandbox tool commands".Translate();
    }

    public string[] GetUsage()
    {
        return ["sandbox", "load-cfg", "self-destruct", "unload-factories"];
    }

    private static async Task DelayedResponse(int time)
    {
        await Task.Delay(time * 1000);
        ThreadingHelper.Instance.StartSyncInvoke(() =>
        {
            ChatManager.Instance.SendChatMessage("Pong", ChatMessageType.CommandOutputMessage);
        });
    }
}
