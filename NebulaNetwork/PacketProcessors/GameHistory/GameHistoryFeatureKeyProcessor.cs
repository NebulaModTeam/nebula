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
    protected override void ProcessPacket(GameHistoryFeatureKeyPacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            Multiplayer.Session.Network.SendPacketExclude(packet, conn);
        }

        using (Multiplayer.Session.History.IsIncomingRequest.On())
        {
            if (packet.Add)
            {
                GameMain.data.history.RegFeatureKey(packet.FeatureId);
            }
            else
            {
                GameMain.data.history.UnregFeatureKey(packet.FeatureId);
            }

            if (packet.FeatureId == 1100002)
            {
                // Update Quick Build button in dyson editor
                UIRoot.instance.uiGame.dysonEditor.controlPanel.inspector.overview.autoConstructSwitch
                    .SetToggleNoEvent(packet.Add);
            }
        }
    }
}
