namespace NebulaModel.Packets.Chat;

public class RemoteServerCommandPacket
{
    public RemoteServerCommandPacket() { }

    public RemoteServerCommandPacket(RemoteServerCommand comand, string content)
    {
        Command = comand;
        Content = content;
    }

    public RemoteServerCommand Command { get; }
    public string Content { get; }
}

public enum RemoteServerCommand
{
    Login,
    ServerList,
    ServerSave,
    ServerLoad,
    ServerInfo
}
