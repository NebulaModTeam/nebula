using NebulaModel.DataStructures;
using NebulaWorld.MonoBehaviours.Local;

namespace NebulaWorld.Chat.Commands
{
    public class XConsoleCommandHandler : IChatCommandHandler
    {
        private readonly XConsole xConsole = new XConsole();

        public void Execute(ChatWindow window, string[] parameters)
        {
            if (parameters.Length > 0)
            {
                string commandText = string.Join(" ", parameters);
                xConsole.ExecuteCommand(commandText);
                string output = xConsole.consoleText;
                if (output.EndsWith("Bad command.</color>\r\n"))
                {
                    output = $">> {commandText}\n>> Bad command. Use /x -help to get list of known commands.";
                    window.SendLocalChatMessage(output, ChatMessageType.CommandErrorMessage);
                }
                else
                {
                    window.SendLocalChatMessage(xConsole.consoleText, ChatMessageType.CommandOutputMessage);
                }
                xConsole.consoleText = "";
                xConsole.history_cmds.Clear();
            }
        }

        public string GetDescription()
        {
            return "Execute developer console command";
        }

        public string[] GetUsage()
        {
            return new string[] { "[XConsole command]" };
        }
    }
}