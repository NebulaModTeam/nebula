#region

using NebulaAPI.Packets;
using NebulaModel.DataStructures.Chat;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat;
using NebulaWorld;
using NebulaWorld.MonoBehaviours.Local.Chat;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat;

[RegisterPacketProcessor]
public class CombatTruceUpdateProcessor : PacketProcessor<CombatTruceUpdatePacket>
{
    protected override void ProcessPacket(CombatTruceUpdatePacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            Multiplayer.Session.Network.SendPacketExclude(packet, conn);
        }

        var truceTime = packet.TruceEndTime - (GameMain.gameTick + GameMain.history.dfTruceTimer);
        GameMain.history.AddTruceTime(truceTime);

        var userName = "";
        using (Multiplayer.Session.World.GetRemotePlayersModels(out var remotePlayersModels))
        {
            if (remotePlayersModels.TryGetValue(packet.PlayerId, out var player))
            {
                userName = player.Username;
            }
        }
        var message = userName + " set ";
        var second = (int)(GameMain.history.dfTruceTimer / 60L);
        var minute = second / 60;
        var hour = minute / 60;
        message += string.Format("停战时间".Translate(), hour, minute % 60, second % 60);
        ChatManager.Instance.SendChatMessage(message, ChatMessageType.BattleMessage);
    }
}
