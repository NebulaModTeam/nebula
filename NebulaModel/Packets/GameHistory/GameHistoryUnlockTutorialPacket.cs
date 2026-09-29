namespace NebulaModel.Packets.GameHistory;

public class GameHistoryUnlockTutorialPacket
{
    public GameHistoryUnlockTutorialPacket() { }

    public GameHistoryUnlockTutorialPacket(int tutorialId)
    {
        TutorialId = tutorialId;
    }

    public int TutorialId { get; set; }
}
