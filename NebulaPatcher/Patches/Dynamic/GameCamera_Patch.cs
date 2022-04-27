using HarmonyLib;
using NebulaWorld;
using NebulaWorld.GameStates;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GameCamera))]
    public class GameCamera_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameCamera.Logic))]
        public static bool Logic_Prefix()
        {
            // prevent NRE while doing a reconnect as a client issued through the chat command
            return !(GameStatesManager.DuringReconnect && GameMain.mainPlayer == null);
        }
    }
}
