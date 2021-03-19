using HarmonyLib;

namespace NebulaPatcher.Patches.Dynamic
{
    // This is part of the weird planet movement fix
    // as we delay setting localPlanet and planetId we need to be able to load FactoryData without them beeing set
    // LoadingPlanetFactoryMain() -> factoryModel.gpuiManager.AddModel -> VegeRenderer::AddInst() -> base.manager.activeFactory -> GPUInstancingManager::get_activeFactory() -> get_activePlanet()
    // which accesses GameMain.localPlanet which is null in that case which causes a NullReferenceException
    // we need to return the right PlanetData in that case, luckily we make use of PlanetData.factoryLoading while loading the factory
    [HarmonyPatch(typeof(GPUInstancingManager), "get_activePlanet")]
    class GPUInstancingManager_Patch
    {
        public static bool Prefix(GPUInstancingManager __instance, ref PlanetData __result)
        {
            __result = (__instance.specifyPlanet != null) ? __instance.specifyPlanet : GameMain.localPlanet;
            if (__result == null && GameMain.localStar != null)
            {
                foreach (PlanetData p in GameMain.galaxy.StarById(GameMain.localStar.id).planets)
                {
                    if (p.factoryLoading)
                    {
                        __result = p;
                        break;
                    }
                }
            }
            return false;
        }
    }
}
