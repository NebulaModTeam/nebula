using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Factory;
using NebulaWorld;
using NebulaWorld.Factory;
using System.Collections.Generic;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PlayerAction_Build))]
    class PlayerAction_Build_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("DoDismantleObject")]
        public static bool DoDestructObject_Prefix(PlayerAction_Build __instance, int objId)
        {
            if (!SimulatedWorld.Initialized)
                return true;

            //Clients needs to send destruction packet here
            if (!LocalPlayer.IsMasterClient && !FactoryManager.EventFromServer && !FactoryManager.EventFromClient)
            {
                LocalPlayer.SendPacket(new DestructEntityRequest(__instance.player.planetId, objId, LocalPlayer.PlayerId));
            }

            return LocalPlayer.IsMasterClient || FactoryManager.EventFromServer;
        }
      
        //[HarmonyPrefix]
        //[HarmonyPatch("AfterPrebuild")]
        public static bool AfterPrebuild_Prefix()
        {
            return !FactoryManager.EventFromServer && !FactoryManager.EventFromClient;
        }
    }
}
