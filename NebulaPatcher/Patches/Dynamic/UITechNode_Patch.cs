#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UITechNode))]
public class UITechNode_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UITechNode.UpdateInfoDynamic))]
    public static void UpdateInfoDynamic_Postfix(UITechNode __instance)
    {
        // Always disable the buyout button for clients.
        if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsClient)
        {
            __instance.buyoutButton.transitions[0].normalColor = __instance.buyoutNormalColor1;
            __instance.buyoutButton.transitions[0].mouseoverColor = __instance.buyoutMouseOverColor1;
            __instance.buyoutButton.transitions[0].pressedColor = __instance.buyoutPressedColor1;
            //__instance.buyoutButton.gameObject.SetActive(false);
        }
    }

    // Always disable the buyout button for clients.
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UITechNode.OnBuyoutButtonClick))]
    public static bool OnBuyoutButtonClick_Prefix(int _data)
    {
        if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsClient)
        {
            UIRealtimeTip.Popup("Only the host can do this!");
            return false;
        }
        return true;
    }
}
