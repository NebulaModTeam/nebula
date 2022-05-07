namespace NebulaModel.Packets.Players
{
    public class AdminCommandPacket
    {
        public AdminCommand Command { get; set; }
        public string Content { get; set; }

        public AdminCommandPacket () {}

        public AdminCommandPacket(AdminCommand comand, string content) 
        {
            Command = comand;
            Content = content;
        }
    }

    public enum AdminCommand
    {
        None = 0,
        ServerSave = 1
    }
}
