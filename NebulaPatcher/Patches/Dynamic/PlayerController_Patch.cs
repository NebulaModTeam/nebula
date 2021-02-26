using HarmonyLib;
using NebulaClient.MonoBehaviours;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PlayerController), "SetReady")]
    class PlayerController_Patch
    {
        public static void Postfix()
        {
            PlayerNetworked player = GameMain.mainPlayer.gameObject.GetComponent<PlayerNetworked>();
            if (!player)
            {
                player = GameMain.mainPlayer.gameObject.AddComponent<PlayerNetworked>();
            }

            player.Init();
        }
    }
}
