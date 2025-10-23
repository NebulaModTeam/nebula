#region

using System;
using NebulaModel;
using NebulaModel.DataStructures.Chat;
using NebulaWorld.MonoBehaviours.Local.Chat;
using NebulaWorld.Planet;

#endregion

namespace NebulaWorld.Chat.Commands;

public class DevCommandHandler : IChatCommandHandler
{
    public void Execute(ChatWindow window, string[] parameters)
    {
        if (parameters.Length < 1)
        {
            throw new ChatCommandUsageException("Not enough arguments!".Translate());
        }

        switch (parameters[0])
        {
            case "sandbox":
                {
                    GameMain.sandboxToolsEnabled = !GameMain.sandboxToolsEnabled;
                    GameMain.data.gameDesc.isSandboxMode = GameMain.sandboxToolsEnabled;
                    window.SendLocalChatMessage("SandboxTool enable: " + GameMain.sandboxToolsEnabled, ChatMessageType.CommandOutputMessage);
                    return;
                }
            case "load-cfg":
                {
                    // Adjust combat settings or make resources infinite
                    window.SendLocalChatMessage("Overwrite settings from nebulaGameDescSettings.cfg", ChatMessageType.CommandOutputMessage);                    
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
                        window.SendLocalChatMessage("this command is only available for client", ChatMessageType.CommandOutputMessage);
                    }
                    else if (GameMain.localPlanet != null)
                    {
                        window.SendLocalChatMessage("can only unload when in space", ChatMessageType.CommandOutputMessage);
                    }
                    else
                    {
                        var factoryCount = GameMain.data.factoryCount;
                        PlanetManager.UnloadAllFactories();
                        window.SendLocalChatMessage($"unload factory count: {factoryCount}", ChatMessageType.CommandOutputMessage);
                    }
                    return;
                }

            case "trigger-error":
                {
                    throw new NullReferenceException();
                }

            default:
                window.SendLocalChatMessage("Unknown command: " + parameters[0], ChatMessageType.CommandOutputMessage);
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
}
