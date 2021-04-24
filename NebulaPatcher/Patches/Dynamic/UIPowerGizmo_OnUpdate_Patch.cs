using HarmonyLib;
using NebulaWorld;

/*
 * This Patch is part of the remote factory loading process.
 * As the client requests the factory the localPlanet is already set by the ArrivePlanet() method.
 * As UIPowerGizmo::_OnUpdate() lacks a null check on the factory of the localPlanet we need to do that here
 * because the factory data may not be received/loaded by this time.
 */
namespace NebulaPatcher.Patches.Dynamic
{
    class UIPowerGizmo_OnUpdate_Patch
    {
        [HarmonyPatch(typeof(UIPowerGizmo))]
        class OnUpdatePatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("_OnUpdate")]
            public static bool _OnUpdate_Prefix()
            {
                return !SimulatedWorld.Initialized || GameMain.localPlanet?.factory != null;
            }
        }
    }
}
