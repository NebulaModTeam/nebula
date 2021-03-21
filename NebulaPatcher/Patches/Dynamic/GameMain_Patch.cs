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
                if (MultiplayerHostSession.IsDedicated && !isServerAlreadyRunning)
                { 
                    int port = 8469;
                    Log.Info($"Listening server on port {port}");
                    var session = NebulaBootstrapper.Instance.CreateMultiplayerHostSession();
                    session.StartServer(port);
                    string loadfile = "";

                    //reading parameters
                    string[] args = System.Environment.GetCommandLineArgs();
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (args[i] == "-load" && i+1 < args.Length)
                        {
                            loadfile = args[i + 1];
                        }
                    }

                    if (loadfile == "") {
                        GameDesc gameDesc = new GameDesc();
                        gameDesc.SetForNewGame(UniverseGen.algoVersion, 1, 64, 1, 1f);
                        DSPGame.StartGameSkipPrologue(gameDesc);
                    } else
                    {
                        DSPGame.StartGame(loadfile);
                    }
                    Debug.Log("Starting server!");
                }

        }
    }
}
