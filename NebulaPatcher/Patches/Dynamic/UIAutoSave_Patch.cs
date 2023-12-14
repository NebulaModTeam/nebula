#region

using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIAutoSave))]
internal class UIAutoSave_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIAutoSave._OnOpen))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnOpen_Postfix(UIAutoSave __instance)
    {
        // Hide AutoSave failed message on clients, since client cannot save in multiplayer
        var contentCanvas = __instance.contentCanvas;
        if (contentCanvas == null)
        {
            return;
        }

        GameObject gameObject;
        (gameObject = contentCanvas.gameObject).SetActive(!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost);
        Log.Warn($"UIAutoSave active: {gameObject.activeSelf}");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIAutoSave._OnLateUpdate))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static bool _OnLateUpdate_Prefix()
    {
        return !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost;
    }
}
