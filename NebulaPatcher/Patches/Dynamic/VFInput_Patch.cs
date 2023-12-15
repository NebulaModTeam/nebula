#region

using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using NebulaWorld;
using NebulaWorld.MonoBehaviours.Local.Chat;
using TMPro;
using UnityEngine.EventSystems;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(VFInput))]
internal class VFInput_Patch
{
    [HarmonyPatch(nameof(VFInput._buildConfirm), MethodType.Getter)]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static bool _buildConfirm_Prefix(ref VFInput.InputValue __result)
    {
        if (!Multiplayer.IsActive || !Multiplayer.Session.Factories.IsIncomingRequest.Value)
        {
            return true;
        }
        __result = default;
        __result.onDown = true;
        return false;
    }

    [HarmonyPatch(nameof(VFInput.UpdateGameStates))]
    [HarmonyPostfix]
    public static void UpdateGameStates_Postfix()
    {
        if (!VFInput.inputing)
        {
            var currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
            VFInput.inputing = currentSelectedGameObject != null &&
                               currentSelectedGameObject.GetComponent<TMP_InputField>() != null;
        }

        if (!VFInput.inScrollView && ChatManager.Instance != null && EmojiPicker.instance != null)
        {
            VFInput.inScrollView = ChatManager.Instance.IsPointerIn() || EmojiPicker.instance.pointerIn;
        }
    }
}
