namespace NebulaModel.DataStructures
{
    public enum ChatMessageType
    {
        PlayerMessage = 0,
        SystemMessage = 1,
        CommandUsageMessage = 2,
        CommandOutputMessage = 3,
        CommandErrorMessage = 4,
        PlayerMessagePrivate = 5
    }
}