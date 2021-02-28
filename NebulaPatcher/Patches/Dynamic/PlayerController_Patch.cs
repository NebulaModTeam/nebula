using HarmonyLib;
using NebulaClient.Extensions;
using NebulaClient.MonoBehaviours.Local;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PlayerController), "SetReady")]
    class PlayerController_Patch
    {
        // Called at the start of a game on the local player controller
        public static void Postfix()
        {
            // Make sure to add the local player components to his character to be able to replicate its behaviour on all clients.
            GameMain.mainPlayer.gameObject.AddComponentIfMissing<LocalPlayerMovement>();
            GameMain.mainPlayer.gameObject.AddComponentIfMissing<LocalPlayerAnimation>();
        }
    }
}
