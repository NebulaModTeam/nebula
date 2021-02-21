using BepInEx;
using HarmonyLib;
using Nebula.UI;

namespace Nebula
{
    [BepInPlugin("com.github.hubertgendron.nebula", "Nebula - Multiplayer Mod", "0.0.0.1")]
    [BepInProcess("DSPGAME.exe")]
    public class NebulaPlugin : BaseUnityPlugin
    {
        void Awake()
        {
            Logger.LogInfo(UnityEngine.Application.unityVersion);

            Harmony.CreateAndPatchAll(typeof(UIMainMenuPatcher), null);
        }
    }
}
