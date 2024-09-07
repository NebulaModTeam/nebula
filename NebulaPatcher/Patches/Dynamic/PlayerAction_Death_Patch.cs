#region

using HarmonyLib;
using NebulaModel.Packets.Combat.GroundEnemy;
using NebulaModel.Packets.Combat.Mecha;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(PlayerAction_Death))]
internal class PlayerAction_Death_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerAction_Death.Respawn))]
    public static void Respawn_Prefix(PlayerAction_Death __instance, int _respawnMode)
    {
        if (!Multiplayer.IsActive || __instance.player != GameMain.mainPlayer) return;

        if (!__instance.player.isAlive && !__instance.respawning)
        {
            Multiplayer.Session.Network.SendPacket(new MechaAliveEventPacket(
                Multiplayer.Session.LocalPlayer.Id, (MechaAliveEventPacket.EStatus)_respawnMode));
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerAction_Death.SettleRespawnCost))]
    public static bool SettleRespawnCost()
    {
        // Don't cost metadata in Multiplayer
        return !Multiplayer.IsActive;
    }
}
