#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameHistory;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.GameHistory;

[RegisterPacketProcessor]
public class GameHistoryFeatureKeyProcessor : PacketProcessor<GameHistoryFeatureKeyPacket>
{
    private const int MAX_ADVISOR_TIP_ID = FeatureID.ADVISOR_TIP_USED_START - FeatureID.ADVISOR_TIP_START; // 1000
    private const int ADVISOR_TIP_USED_END = FeatureID.ADVISOR_TIP_USED_START + MAX_ADVISOR_TIP_ID; // 2002000

    protected override void ProcessPacket(GameHistoryFeatureKeyPacket packet, NebulaConnection conn)
    {
        if (packet == null)
        {
            return;
        }

        if (IsHost)
        {
            Multiplayer.Session.Network.SendPacketExclude(packet, conn);
        }

        using (Multiplayer.Session.History.IsIncomingRequest.On())
        {
            if (packet.Add)
            {
                GameMain.data?.history?.RegFeatureKey(packet.FeatureId);

                if (packet.FeatureId >= FeatureID.ADVISOR_TIP_START && packet.FeatureId < FeatureID.ADVISOR_TIP_USED_START)
                {
                    int tipId = packet.FeatureId - FeatureID.ADVISOR_TIP_START;
                    GameMain.gameScenario?.advisorLogic?.SetAdvisorTipFinished(tipId);
                    DismissAdvisorTipUI(tipId);
                }
                else if (packet.FeatureId >= FeatureID.ADVISOR_TIP_USED_START && packet.FeatureId < ADVISOR_TIP_USED_END)
                {
                    int tipId = packet.FeatureId - FeatureID.ADVISOR_TIP_USED_START;
                    GameMain.gameScenario?.advisorLogic?.SetAdvisorTipUsed(tipId);
                    DismissAdvisorTipUI(tipId);
                }
            }
            else
            {
                GameMain.data?.history?.UnregFeatureKey(packet.FeatureId);
            }

            if (packet.FeatureId == 1100002)
            {
                // Update Quick Build button in dyson editor
                UIRoot.instance.uiGame.dysonEditor.controlPanel.inspector.overview.autoConstructSwitch
                    .SetToggleNoEvent(packet.Add);
            }
        }
    }

    private static void DismissAdvisorTipUI(int tipId)
    {
        var advisorTip = UIRoot.instance?.uiGame?.advisorTip;
        if (advisorTip == null)
        {
            return;
        }

        if (advisorTip.playingTip?.ID == tipId)
        {
            advisorTip.StopAdvisorTip();
        }
        advisorTip.requests?.RemoveAll(id => id == tipId);
        if (advisorTip.nextTip?.ID == tipId)
        {
            advisorTip.nextTip = null;
        }
    }
}
