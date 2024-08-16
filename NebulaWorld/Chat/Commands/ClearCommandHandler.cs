#region

using NebulaModel.Utils;
using NebulaWorld.MonoBehaviours.Local.Chat;

#endregion

namespace NebulaWorld.Chat.Commands;

public class ClearCommandHandler : IChatCommandHandler
{
    public void Execute(ChatWindow window, string[] parameters)
    {
        if (parameters.Length > 0)
        {
            var param = parameters[0];
            if (param.Equals("all"))
            {
                window.ClearChat();
                return;
            }
        }
        window.ClearChat(message => !message.MessageType.IsPlayerMessage());
    }

    public string[] GetUsage()
    {
        return new[] { "[all|commands]" };
    }

    public string GetDescription()
    {
        return "Clear all chat messages (locally)".Translate();
    }
}
