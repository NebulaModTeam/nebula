using HarmonyLib;
using NebulaNetwork;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GameLoader))]
    class GameLoader_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameLoader.FixedUpdate))]
        public static void FixedUpdate_Postfix(int ___frame)
        {
            if (___frame >= 11 && SimulatedWorld.Instance.Initialized)
            {
                SimulatedWorld.Instance.OnGameLoadCompleted();
                if (!LocalPlayer.Instance.IsMasterClient)
                {
                    MultiplayerClientSession.Instance.DisplayPingIndicator();
                }
            }
        }
    }
}
