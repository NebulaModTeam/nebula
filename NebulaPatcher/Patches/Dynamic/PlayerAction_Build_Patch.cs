using HarmonyLib;
using NebulaModel.Packets.Factory;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PlayerAction_Build))]
    class PlayerAction_Build_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerAction_Build.SetFactoryReferences))]
        public static bool SetFactoryReferences_Prefix()
        {
            if((FactoryManager.EventFromServer || FactoryManager.EventFromClient) && FactoryManager.PacketAuthor != LocalPlayer.PlayerId && FactoryManager.TargetPlanet != GameMain.localPlanet?.id)
            {
                return false;
            }
            return true;
        }
    }
}
