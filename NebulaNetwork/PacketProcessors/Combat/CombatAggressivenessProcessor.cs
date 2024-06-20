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
public class CombatAggressivenessProcessor : PacketProcessor<CombatAggressivenessUpdatePacket>
{
    protected override void ProcessPacket(CombatAggressivenessUpdatePacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            Multiplayer.Session.Network.SendPacketExclude(packet, conn);
        }

        var history = GameMain.history;
        var oldAggressiveLevel = history.combatSettings.aggressiveLevel;
        history.combatSettings.aggressiveness = packet.Aggressiveness;

        var currentPropertyMultiplier = history.currentPropertyMultiplier;
        if (currentPropertyMultiplier < history.minimalPropertyMultiplier)
        {
            history.minimalPropertyMultiplier = currentPropertyMultiplier;
        }
        var difficulty = history.combatSettings.difficulty;
        if (difficulty < history.minimalDifficulty)
        {
            history.minimalDifficulty = difficulty;
        }
        GameMain.data.spaceSector.OnDFAggressivenessChanged(oldAggressiveLevel, history.combatSettings.aggressiveLevel);

        var userName = "";
        using (Multiplayer.Session.World.GetRemotePlayersModels(out var remotePlayersModels))
        {
            if (remotePlayersModels.TryGetValue(packet.PlayerId, out var player))
            {
                userName = player.Username;
            }
        }
        var message = string.Format("{0} set DF Aggressiveness {1} => {2}".Translate(), userName, oldAggressiveLevel, history.combatSettings.aggressiveLevel);
        ChatManager.Instance.SendChatMessage(message, ChatMessageType.BattleMessage);
    }
}
