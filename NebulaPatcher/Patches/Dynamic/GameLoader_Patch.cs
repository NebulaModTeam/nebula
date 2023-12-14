#region

using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;
using NebulaWorld.GameStates;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(GameLoader))]
internal class GameLoader_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameLoader.FixedUpdate))]
    public static void FixedUpdate_Postfix(int ___frame)
    {
        var content = GameStatesManager.FragmentSize > 0 ? GameStatesManager.LoadingMessage() : Log.LastInfoMsg;
        InGamePopup.UpdateMessage("Loading".Translate(), "Loading state from server, please wait".Translate() + "\n" + content);
        if (Multiplayer.IsActive && ___frame >= 11)
        {
            Multiplayer.Session.OnGameLoadCompleted();
        }
    }
}
