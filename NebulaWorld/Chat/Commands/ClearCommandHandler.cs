#region

using NebulaModel.Utils;

#endregion

namespace NebulaWorld.Chat.Commands;

public class ClearCommandHandler : IChatCommandHandler
{
    public void Execute(ChatService chatService, string[] parameters)
    {
        if (parameters.Length > 0)
        {
            var param = parameters[0];
            if (param.Equals("all"))
            {
                chatService.ClearMessages(_ => true);
                return;
            }
        }
        chatService.ClearMessages(message => !message.MessageType.IsPlayerMessage());
    }

    public string[] GetUsage()
    {
        return ["[all|commands]"];
    }

    public string GetDescription()
    {
        return "Clear all chat messages (locally)".Translate();
    }
}
