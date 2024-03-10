namespace NebulaModel.DataStructures.Chat;

public enum ChatMessageType
{
    PlayerMessage = 0,
    SystemInfoMessage = 1,
    SystemWarnMessage = 2,
    BattleMessage = 3,
    CommandUsageMessage = 4,
    CommandOutputMessage = 5,
    CommandErrorMessage = 6,
    PlayerMessagePrivate = 7
}
