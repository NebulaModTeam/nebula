#region

using System.Threading.Tasks;
using BepInEx;
using NebulaModel.DataStructures.Chat;
using NebulaWorld.MonoBehaviours.Local.Chat;

#endregion

#pragma warning disable 4014

namespace NebulaWorld.Chat.Commands;

public class PingCommandHandler : IChatCommandHandler
{
    public void Execute(ChatWindow window, string[] parameters)
    {
        if (parameters.Length > 0)
        {
            if (int.TryParse(parameters[0], out var value))
            {
                DelayedResponse(value);
                return;
            }
        }

        window.SendLocalChatMessage("Pong", ChatMessageType.CommandOutputMessage);
    }

    public string GetDescription()
    {
        return "Test command".Translate();
    }

    public string[] GetUsage()
    {
        return new[] { "[time delay (seconds)]" };
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
