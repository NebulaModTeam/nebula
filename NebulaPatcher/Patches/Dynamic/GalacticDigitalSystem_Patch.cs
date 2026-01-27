#region

using System;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(GalacticDigitalSystem))]
internal class GalacticDigitalSystem_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GalacticDigitalSystem.Import))]
    public static void Import_Postfix(GalacticDigitalSystem __instance)
    {
        // Only run on client - host doesn't need this refresh since they created the markers
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return;
        }

        // Refresh marker rendering after import to ensure visual holograms are created
        // Use reflection since RecollectMarkerData location varies between game versions
        try
        {
            var bindingFlags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance;
            var instanceType = __instance.GetType();

            var recollectMethod = instanceType.GetMethod("RecollectMarkerData", bindingFlags);
            if (recollectMethod != null)
            {
                recollectMethod.Invoke(__instance, null);
                Log.Debug("RecollectMarkerData called on GalacticDigitalSystem in Import_Postfix");
            }
            else
            {
                var markerRendererField = instanceType.GetField("markerRenderer", bindingFlags);
                if (markerRendererField != null)
                {
                    var markerRenderer = markerRendererField.GetValue(__instance);
                    if (markerRenderer != null)
                    {
                        var rendererRecollectMethod = markerRenderer.GetType().GetMethod("RecollectMarkerData", bindingFlags);
                        if (rendererRecollectMethod != null)
                        {
                            rendererRecollectMethod.Invoke(markerRenderer, null);
                            Log.Debug("RecollectMarkerData called on markerRenderer in Import_Postfix");
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Warn($"RecollectMarkerData in Import_Postfix failed: {e.Message}");
        }
    }
}
