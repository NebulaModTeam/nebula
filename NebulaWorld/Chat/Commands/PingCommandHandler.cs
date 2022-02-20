using BepInEx;
using NebulaModel.DataStructures;
using NebulaWorld.MonoBehaviours.Local;
using System.Threading.Tasks;
#pragma warning disable 4014

namespace NebulaWorld.Chat.Commands
{
    public class PingCommandHandler : IChatCommandHandler
    {
        public void Execute(ChatWindow window, string[] parameters)
        {
            if (parameters.Length > 0)
            {
                if (int.TryParse(parameters[0], out int value))
                {
                    DelayedResponse(value);
                    return;
                }
            }
            
            window.SendLocalChatMessage("Pong", ChatMessageType.CommandOutputMessage);
        }

        private async Task DelayedResponse(int time)
        {
            await Task.Delay(time * 1000);
            ThreadingHelper.Instance.StartSyncInvoke(() =>
            {
                ChatManager.Instance.SendChatMessage("Pong", ChatMessageType.CommandOutputMessage);
            });
            
        }

        public string GetDescription()
        {
            return "Test command";
        }

        public string GetUsage()
        {
            return "[time delay (seconds)]";
        }
    }
}