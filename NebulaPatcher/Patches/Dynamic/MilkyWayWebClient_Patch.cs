#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(MilkyWayWebClient))]
internal class MilkyWayWebClient_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(MilkyWayWebClient.canUploadGame), MethodType.Getter)]
    public static void Get_canUploadGame_Postfix(ref bool __result)
    {
        // We don't want to upload Milky Way data if we are playing MP
        __result &= !Multiplayer.IsActive;
    }
}
