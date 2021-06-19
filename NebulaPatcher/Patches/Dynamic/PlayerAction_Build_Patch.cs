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
        [HarmonyPatch("DoDismantleObject")]
        public static bool DoDismantleObject_Prefix(PlayerAction_Build __instance, int objId)
        {
            if (!SimulatedWorld.Initialized)
                return true;

            BuildTool[] buildTools = __instance.tools;
            BuildTool buildTool = null;

            for(int i = 0; i < buildTools.Length; i++)
            {
                if(buildTools[i].GetType().ToString() == "BuildTool_Path")
                {
                    buildTool = buildTools[i];
                    break;
                }
            }

            //Clients needs to send destruction packet here
            if (!LocalPlayer.IsMasterClient && !FactoryManager.EventFromServer && !FactoryManager.EventFromClient)
            {
                LocalPlayer.SendPacket(new DestructEntityRequest(__instance.player.planetId, objId, LocalPlayer.PlayerId));
            }
            else if(!LocalPlayer.IsMasterClient && FactoryManager.EventFromServer && !FactoryManager.EventFromClient && FactoryManager.TargetPlanet == __instance.planet.id && buildTool.ObjectIsBelt(objId))
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
