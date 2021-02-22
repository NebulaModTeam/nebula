using BepInEx;
using HarmonyLib;
using NebulaClient.MonoBehaviours;
using NebulaModel.Logger;
using NebulaPatcher.Patches;
using NebulaPatcher.Patches.Dynamic;
using System;
using System.Linq;
using UnityEngine;

namespace NebulaPatcher
{
    [BepInPlugin("com.github.hubertgendron.nebula", "Nebula - Multiplayer Mod", "0.0.0.1")]
    [BepInProcess("DSPGAME.exe")]
    public class NebulaPlugin : BaseUnityPlugin
    {
        Harmony harmony = new Harmony("com.github.hubertgendron.nebula");

        void Awake()
        {
            Log.Setup(Logger);

            try
            {
                Initialize();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled exception occurred while initializing Nebula:");
            }
        }

        void Initialize()
        {
            InitPatches();
            ApplyNebulaBehaviours();
        }

        private void InitPatches()
        {
            Log.Info("Patching Dyson Sphere Program...");

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(var assembly in assemblies)
            {
                if (assembly.FullName.StartsWith("Nebula,"))
                {
                    Log.Info($"Applying patches from assembly: {assembly.FullName}");
                    harmony.PatchAll(assembly);
                }
            }

            /*
            Multiplayer.OnBeforeMultiplayerStart += Apply;
            Multiplayer.OnAfterMultiplayerEnd += Restore;
            */

            Log.Info("Completed patching");
        }

        void ApplyNebulaBehaviours()
        {
            Log.Info("Applying Nebula behaviours..");

            GameObject nebulaRoot = new GameObject();
            nebulaRoot.name = "Nebula";
            nebulaRoot.AddComponent<NebulaBootstrapper>();

            Log.Info("Behaviours applied.");
        }
    }
}
