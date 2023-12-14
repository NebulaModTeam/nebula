namespace NebulaModel.DataStructures.Chat;

public enum ChatMessageType
{
    PlayerMessage = 0,
    SystemInfoMessage = 1,
    SystemWarnMessage = 2,
    CommandUsageMessage = 3,
    CommandOutputMessage = 4,
    CommandErrorMessage = 5,
    PlayerMessagePrivate = 6
}
