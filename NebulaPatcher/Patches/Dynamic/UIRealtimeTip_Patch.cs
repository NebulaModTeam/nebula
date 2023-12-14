#region

using HarmonyLib;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIRealtimeTip))]
internal class UIRealtimeTip_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIRealtimeTip.Popup), typeof(string), typeof(bool), typeof(int))]
    public static bool Popup_Prefix()
    {
        //Do not show popups if they are triggered remotely
        return !Multiplayer.IsActive || !Multiplayer.Session.Factories.IsIncomingRequest.Value;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIRealtimeTip.Popup), typeof(string), typeof(Vector2), typeof(int))]
    public static bool Popup_Prefix2()
    {
        //Do not show popups if they are triggered remotely
        return !Multiplayer.IsActive || !Multiplayer.Session.Factories.IsIncomingRequest.Value;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIRealtimeTip.Popup), typeof(string), typeof(Vector3), typeof(bool), typeof(int))]
    public static bool Popup_Prefix3()
    {
        //Do not show popups if they are triggered remotely
        return !Multiplayer.IsActive || !Multiplayer.Session.Factories.IsIncomingRequest.Value;
    }
}
