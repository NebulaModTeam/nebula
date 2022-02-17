using HarmonyLib;
using NebulaWorld;
using NebulaWorld.MonoBehaviours.Local;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(VFInput))]
    internal class VFInput_Patch
    {
        [HarmonyPatch(nameof(VFInput._buildConfirm), MethodType.Getter)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static bool _buildConfirm_Prefix(ref VFInput.InputValue __result)
        {
            if (Multiplayer.IsActive && Multiplayer.Session.Factories.IsIncomingRequest.Value)
            {
                __result = default;
                __result.onDown = true;
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(VFInput.UpdateGameStates))]
        [HarmonyPostfix]
        public static void UpdateGameStates_Postfix()
        {
            if (!VFInput.inputing)
            {
                GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
                VFInput.inputing = currentSelectedGameObject != null && currentSelectedGameObject.GetComponent<TMP_InputField>() != null;
            }

            if (!VFInput.inScrollView && ChatManager.Instance != null && EmojiPicker.instance != null)
            {
                VFInput.inScrollView = ChatManager.Instance.IsPointerIn() || EmojiPicker.instance.pointerIn;
            }
        }
    }
}
