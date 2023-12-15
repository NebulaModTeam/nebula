#region

using HarmonyLib;
using NebulaAPI;
using NebulaModel.Packets.Universe;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIStarDetail))]
internal class UIStarDetail_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIStarDetail.OnNameInputEndEdit))]
    public static void OnNameInputEndEdit_Postfix(UIStarDetail __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Factories.IsIncomingRequest.Value)
        {
            return;
        }
        if (__instance.star != null && !string.IsNullOrEmpty(__instance.star.overrideName))
        {
            // Send packet with new star name
            Multiplayer.Session.Network.SendPacket(new NameInputPacket(__instance.star.overrideName, __instance.star.id,
                NebulaModAPI.PLANET_NONE));
        }
    }
}
