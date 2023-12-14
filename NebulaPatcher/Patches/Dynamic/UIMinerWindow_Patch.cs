#region

using HarmonyLib;
using NebulaModel.Packets.Factory.Miner;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIMinerWindow))]
internal class UIMinerWindow_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIMinerWindow.OnProductIconClick))]
    public static void OnProductIconClick_Prefix(UIMinerWindow __instance)
    {
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new MinerStoragePickupPacket(__instance.minerId,
                GameMain.localPlanet?.id ?? -1));
        }
    }
}
