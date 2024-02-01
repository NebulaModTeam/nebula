#region

using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using NebulaModel.Packets.Universe;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIStarmap))]
internal class UIStarmap_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIStarmap._OnLateUpdate))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnLateUpdate_Postfix(UIStarmap __instance)
    {
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.World.RenderPlayerNameTagsOnStarmap(__instance);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIStarmap._OnClose))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnClose_Postfix()
    {
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.World.ClearPlayerNameTagsOnStarmap();
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIStarmap.StartFastTravelToUPosition))]
    public static bool TeleportToUPosition_Prefix(VectorLF3 uPos)
    {
        if (!Multiplayer.IsActive || !Multiplayer.Session.LocalPlayer.IsClient)
        {
            return true;
        }
        GameMain.data.QueryNearestStarPlanet(uPos, out var starData, out var planetData);
        if (GameMain.localPlanet == planetData || planetData is not { type: EPlanetType.Gas })
        {
            return true;
        }
        InGamePopup.ShowWarning("Unavailable", "Cannot teleport to gas giant", "OK");
        return false;
    }

    static int s_queryingIndex = -1;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(UIStarmap), nameof(UIStarmap.OnCursorFunction2Click))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void QueryDysonSphere(UIStarmap __instance)
    {
        // Client: Query existing dyson sphere when clicking on 'View' star on starmap
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost || Multiplayer.Session.IsInLobby || __instance.focusStar == null)
        {
            return;
        }

        var starIndex = __instance.focusStar.star.index;
        if (GameMain.data.dysonSpheres[starIndex] == null)
        {
            if (s_queryingIndex != starIndex)
            {
                Multiplayer.Session.Network.SendPacket(new DysonSphereLoadRequest(starIndex, DysonSphereRequestEvent.Query));
                s_queryingIndex = starIndex;
            }
        }
    }
}
