using HarmonyLib;
using NebulaModel;
using NebulaNetwork;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GameSave))]
    internal class GameSave_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameSave.SaveCurrentGame))]
        public static bool SaveCurrentGame_Prefix(string saveName)
        {
            if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
            {
                // temp revert sand count back to original value before saving if we sync it (see SimulatedWorld.SetupInitialPlayerState() )
                if (Config.Options.SyncSoil)
                {
                    int tmp = GameMain.mainPlayer.sandCount;
                    GameMain.mainPlayer.sandCount = Multiplayer.Session.LocalPlayer.Data.Mecha.SandCount;
                    Multiplayer.Session.LocalPlayer.Data.Mecha.SandCount = tmp;
                }
                SaveManager.SaveServerData(saveName);
            }

            // Only save if in single player or if you are the host
            return (!Multiplayer.IsActive && !Multiplayer.IsLeavingGame) || Multiplayer.Session.LocalPlayer.IsHost;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameSave.SaveCurrentGame))]
        public static void SaveCurrentGame_Postfix(string saveName)
        {
            if(Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
            {
                // if we sync soil we need to revert changes from above after saving the game
                if (Config.Options.SyncSoil)
                {
                    int tmp = GameMain.mainPlayer.sandCount;
                    GameMain.mainPlayer.sandCount = Multiplayer.Session.LocalPlayer.Data.Mecha.SandCount;
                    Multiplayer.Session.LocalPlayer.Data.Mecha.SandCount = tmp;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameSave.AutoSave))]
        public static bool AutoSave_Prefix()
        {
            // Only save if in single player or if you are the host
            return !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameSave.SaveAsLastExit))]
        public static bool SaveAsLastExit_Prefix()
        {
            // Only save if in single player, since multiplayer requires to load from the Load Save Window
            return (!Multiplayer.IsActive && !Multiplayer.IsLeavingGame);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameSave.LoadCurrentGame))]
        public static void LoadCurrentGame_Postfix(bool __result)
        {
            // If loading success, check and correct offset for all inserters
            if (__result)
            {
                for (int index = 0; index < GameMain.data.factoryCount; index++)
                {
                    PlanetFactory factory = GameMain.data.factories[index];
                    EntityData[] entityPool = factory.entityPool;
                    CargoTraffic traffic = factory.factorySystem.traffic;
                    BeltComponent[] beltPool = factory.factorySystem.traffic.beltPool;
                    for (int i = 1; i < factory.factorySystem.inserterCursor; i++)
                    {
                        ref InserterComponent inserter = ref factory.factorySystem.inserterPool[i];
                        if (inserter.id == i)
                        {
                            inserter.InternalOffsetCorrection(entityPool, traffic, beltPool);
                        }
                    }
                }
            }
        }
    }
}
