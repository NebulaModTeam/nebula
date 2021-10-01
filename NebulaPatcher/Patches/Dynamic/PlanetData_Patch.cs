using HarmonyLib;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PlanetData))]
    internal class PlanetData_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlanetData.UnloadMeshes))]
        public static bool UnloadMeshes_Prefix(PlanetData __instance)
        {
            //Host should not unload planet meshes, since he need to permorm all terrain operations
            if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
            {
                //Do not unload meshes, just hide them so it is not visible
                UnloadVisuals(__instance);
                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlanetData.UnloadData))]
        public static bool UnloadData_Prefix()
        {
            //Host should not unload planet data, since he need to permorm all operations from users
            if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
            {
                return false;
            }

            return true;
        }

        public static void UnloadVisuals(PlanetData __instance)
        {
            if (__instance.gameObject != null)
            {
                __instance.gameObject.SetActive(false);
            }
            if (__instance.terrainMaterial != null)
            {
                UnityEngine.Object.Destroy(__instance.terrainMaterial);
                __instance.terrainMaterial = null;
            }
            if (__instance.oceanMaterial != null)
            {
                UnityEngine.Object.Destroy(__instance.oceanMaterial);
                __instance.oceanMaterial = null;
            }
            if (__instance.atmosMaterial != null)
            {
                UnityEngine.Object.Destroy(__instance.atmosMaterial);
                __instance.atmosMaterial = null;
            }
            if (__instance.minimapMaterial != null)
            {
                UnityEngine.Object.Destroy(__instance.minimapMaterial);
                __instance.minimapMaterial = null;
            }
            if (__instance.reformMaterial0 != null)
            {
                UnityEngine.Object.Destroy(__instance.reformMaterial0);
                __instance.reformMaterial0 = null;
            }
            if (__instance.reformMaterial1 != null)
            {
                UnityEngine.Object.Destroy(__instance.reformMaterial1);
                __instance.reformMaterial1 = null;
            }
        }
    }
}
