using HarmonyLib;
using NebulaWorld;
using NebulaWorld.GameStates;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GameLoader))]
    internal class GameLoader_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameLoader.FixedUpdate))]
        public static void FixedUpdate_Postfix(int ___frame)
        {
            string content = GameStatesManager.FragmentSize > 0 ? GameStatesManager.LoadingMessage() : NebulaModel.Logger.Log.LastInfoMsg;
            InGamePopup.UpdateMessage("Loading", "Loading state from server, please wait\n" + content);
            if (Multiplayer.IsActive && ___frame >= 11)
            {
                Multiplayer.Session.OnGameLoadCompleted();
            }
        }
    }
}
