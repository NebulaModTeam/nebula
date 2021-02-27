using HarmonyLib;
using NebulaClient.MonoBehaviours.GameLogic;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PlayerController), "SetReady")]
    class PlayerController_Patch
    {
        public static void Postfix()
        {
            LocalPlayer player = GameMain.mainPlayer.gameObject.GetComponent<LocalPlayer>();
            if (!player)
            {
                GameMain.mainPlayer.gameObject.AddComponent<LocalPlayer>();
            }
        }
    }
}
