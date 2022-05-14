namespace NebulaModel.Packets.Players
{
    public class RemoteSaveCommandPacket
    {
        public RemoteSaveCommand Command { get; set; }
        public string Content { get; set; }

        public RemoteSaveCommandPacket () {}

        public RemoteSaveCommandPacket(RemoteSaveCommand comand, string content) 
        {
            Command = comand;
            Content = content;
        }
    }

    public enum RemoteSaveCommand
    {
        ServerList = 0,
        ServerSave = 1,
        ServerLoad = 2
    }
}
