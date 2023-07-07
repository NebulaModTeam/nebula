using NebulaModel.Utils;
using NebulaWorld.MonoBehaviours.Local;

namespace NebulaWorld.Chat.Commands
{
    public class ClearCommandHandler : IChatCommandHandler
    {
        public void Execute(ChatWindow window, string[] parameters)
        {
            if (parameters.Length > 0)
            {
                string param = parameters[0];
                if (param.Equals("all"))
                {
                    window.ClearChat(); 
                    return;
                }
            }
            window.ClearChat(message => message.MessageType.IsCommandMessage());
        }
        
        public string[] GetUsage()
        {
            return new string[] { $"[all|commands]" };
        }

        public string GetDescription()
        {
            return "Clear all chat messages (locally)".Translate();
        }
    }
}