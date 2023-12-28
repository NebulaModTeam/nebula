#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(PowerSystem))]
internal class PowerSystem_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(PowerSystem.GameTick))]
    public static void PowerSystem_GameTick_Prefix(long time, ref bool isActive)
    {
        //Enable signType update on remote planet every 64 tick
        if ((time & 63) == 0 && Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
        {
            isActive = true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PowerSystem.RemoveNodeComponent))]
    public static bool RemoveNodeComponent(PowerSystem __instance, int id)
    {
        if (!Multiplayer.IsActive)
        {
            return true;
        }
        // as the destruct is synced across players this event is too
        // and as such we can safely remove power demand for every player        
        Multiplayer.Session.PowerTowers.LocalChargerIds.Remove(id);
        var hashId = (long)__instance.factory.planetId << 32 | (long)id;
        Multiplayer.Session.PowerTowers.RemoteChargerHashIds.Remove(hashId);

        return true;
    }
}
