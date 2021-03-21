using HarmonyLib;
using NebulaPatcher.MonoBehaviours;
using NebulaModel.Logger;
using NebulaHost;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GameMain), "Begin")]
    class GameMain_Patch
    {
        //Run server in dedicated mode on game start
        public static void Postfix()
        {
            bool isServerAlreadyRunning = NebulaBootstrapper.Instance.GetComponentInChildren<MultiplayerHostSession>() != null;
                if (NebulaBootstrapper.isDedicated && !isServerAlreadyRunning)
                { 
                    int port = 8469;
                    Log.Info($"Listening server on port {port}");
                    var session = NebulaBootstrapper.Instance.CreateMultiplayerHostSession();
                    session.StartServer(port);
                    GameDesc gameDesc = new GameDesc();
                    gameDesc.SetForNewGame(UniverseGen.algoVersion, 1, 64, 1, 1f);
                    DSPGame.StartGameSkipPrologue(gameDesc);
                    Debug.Log("Starting server!");
                }

        }
    }
}
