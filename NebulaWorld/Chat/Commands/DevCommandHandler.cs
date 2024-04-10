#region

using NebulaWorld.MonoBehaviours.Local.Chat;
using NebulaModel.DataStructures.Chat;
using HarmonyLib;

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
                    window.SendLocalChatMessage("Overwrite settings from nebulaGameDescSettings.cfg", ChatMessageType.CommandOutputMessage);
                    AccessTools.Method(AccessTools.TypeByName("NebulaPatcher.NebulaPlugin"), "SetGameDescFromConfigFile").Invoke(null, [GameMain.data.gameDesc]);
                    return;
                }

            case "self-destruct":
                {
                    GameMain.mainPlayer.Kill();
                    return;
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
        return ["sandbox", "load-cfg", "self-destruct"];
    }
}
