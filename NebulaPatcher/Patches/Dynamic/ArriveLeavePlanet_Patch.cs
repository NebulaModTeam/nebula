using HarmonyLib;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GameData))]
    internal class ArrivePlanet_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameData.ArrivePlanet))]
        public static void ArrivePlanet_Postfix(GameData __instance, PlanetData planet)
        {
            if (Multiplayer.IsActive)
            {
                Multiplayer.Session.PlanetRefreshMissingMeshes = true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameData.GameTick))]
        public static void GameTick_Postfix(GameData __instance)
        {
            if (Multiplayer.IsActive && Multiplayer.Session.PlanetRefreshMissingMeshes && __instance.localPlanet != null)
            {
                PlanetData planetData = __instance.localPlanet;

                if (planetData.meshColliders != null)
                {
                    for (int i = 0; i < planetData.meshColliders.Length; i++)
                    {
                        if (planetData.meshColliders[i] != null && planetData.meshColliders[i].sharedMesh == null)
                        {
                            planetData.meshColliders[i].sharedMesh = planetData.meshes[i];
                        }
                    }
                    Multiplayer.Session.PlanetRefreshMissingMeshes = false;
                }
            }
        }

    }
}

