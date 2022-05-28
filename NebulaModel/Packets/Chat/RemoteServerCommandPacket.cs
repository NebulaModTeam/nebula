namespace NebulaModel.Packets.Players
{
    public class RemoteServerCommandPacket
    {
        public RemoteServerCommand Command { get; set; }
        public string Content { get; set; }

        public RemoteServerCommandPacket () {}

        public RemoteServerCommandPacket(RemoteServerCommand comand, string content) 
        {
            Command = comand;
            Content = content;
        }
    }

    public enum RemoteServerCommand
    {
        Login,
        ServerList,
        ServerSave,
        ServerLoad,
        ServerInfo
    }
}
