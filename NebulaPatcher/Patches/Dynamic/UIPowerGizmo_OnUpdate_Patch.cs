#region

using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using NebulaWorld;

#endregion

/*
 * This Patch is part of the remote factory loading process.
 * As the client requests the factory the localPlanet is already set by the ArrivePlanet() method.
 * As UIPowerGizmo::_OnUpdate() lacks a null check on the factory of the localPlanet we need to do that here
 * because the factory data may not be received/loaded by this time.
 */
namespace NebulaPatcher.Patches.Dynamic;

internal static class UIPowerGizmo_OnUpdate_Patch
{
    [HarmonyPatch(typeof(UIPowerGizmo))]
    private class OnUpdatePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIPowerGizmo._OnUpdate))]
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static bool _OnUpdate_Prefix()
        {
            return !Multiplayer.IsActive || GameMain.localPlanet?.factory != null;
        }
    }
}
