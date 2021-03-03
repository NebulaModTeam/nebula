using BepInEx;
using HarmonyLib;
using NebulaClient.MonoBehaviours;
using NebulaModel.Logger;
using NebulaPatcher.Logger;
using System;
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
            Log.Init(new BepInExLogger(Logger));

            try
            {
                Initialize();
            }
            catch (Exception ex)
            {
                Log.Error("Unhandled exception occurred while initializing Nebula:", ex);
            }
        }

        void Initialize()
        {
            InitPatches();
            AddNebulaBootstrapper();
        }

        private void InitPatches()
        {
            Log.Info("Patching Dyson Sphere Program...");

            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    if (assembly.FullName.StartsWith("NebulaPatcher"))
                    {
                        Log.Info($"Applying patches from assembly: {assembly.FullName}");
                        harmony.PatchAll(assembly);
                    }
                }

                Log.Info("Patching completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error("Unhandled exception occurred while patching the game:", ex);
            }
        }

        void AddNebulaBootstrapper()
        {
            Log.Info("Applying Nebula behaviours..");

            GameObject nebulaRoot = new GameObject();
            nebulaRoot.name = "Nebula";
            nebulaRoot.AddComponent<NebulaBootstrapper>();

            Log.Info("Behaviours applied.");
        }
    }
}
