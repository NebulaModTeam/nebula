#region

using NebulaModel.DataStructures.Chat;
using NebulaWorld.GameStates;

#endregion

namespace NebulaWorld.Chat.Commands;

public class ReconnectCommandHandler : IChatCommandHandler
{
    public void Execute(ChatService chatService, string[] parameters)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            chatService.AddMessage("This command can only be used in multiplayer and as client!".Translate(),
                ChatMessageType.CommandErrorMessage);
            return;
        }
        GameStatesManager.DoFastReconnect();
    }

    public string[] GetUsage()
    {
        return [""];
    }

    public string GetDescription()
    {
        return "Perform a reconnect.".Translate();
    }
}
