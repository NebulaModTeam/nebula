#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(GuideMissionStandardMode))]
internal class GuideMissionStandardMode_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GuideMissionStandardMode.Skip))]
    public static bool Skip_Prefix()
    {
        //This prevents spawning landing capsule and preparing spawn area for the clients in multiplayer.
        return !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost;
    }
}
