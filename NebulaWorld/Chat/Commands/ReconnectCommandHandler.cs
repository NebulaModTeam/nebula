#region

using NebulaModel.DataStructures.Chat;
using NebulaWorld.GameStates;
using NebulaWorld.MonoBehaviours.Local.Chat;

#endregion

namespace NebulaWorld.Chat.Commands;

public class ReconnectCommandHandler : IChatCommandHandler
{
    public void Execute(ChatWindow window, string[] parameters)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            window.SendLocalChatMessage("This command can only be used in multiplayer and as client!".Translate(),
                ChatMessageType.CommandErrorMessage);
            return;
        }
        GameStatesManager.DoFastReconnect();
    }

    public string[] GetUsage()
    {
        return new[] { "" };
    }

    public string GetDescription()
    {
        return "Perform a reconnect.".Translate();
    }
}
