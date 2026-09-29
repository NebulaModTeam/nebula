#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameHistory;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.GameHistory;

[RegisterPacketProcessor]
public class GameHistoryUnlockTutorialProcessor : PacketProcessor<GameHistoryUnlockTutorialPacket>
{
    protected override void ProcessPacket(GameHistoryUnlockTutorialPacket packet, NebulaConnection conn)
    {
        if (packet == null || packet.TutorialId <= 0)
        {
            return;
        }

        if (IsHost)
        {
            Multiplayer.Session.Network.SendPacketExclude(packet, conn);
        }

        using (Multiplayer.Session.History.IsIncomingRequest.On())
        {
            GameMain.data?.history?.UnlockTutorial(packet.TutorialId);
            UIRoot.instance?.uiGame?.tutorialTip?.CloseTip(packet.TutorialId);
        }
    }
}
