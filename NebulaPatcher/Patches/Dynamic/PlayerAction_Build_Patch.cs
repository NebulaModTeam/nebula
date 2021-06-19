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

            //Clients needs to send destruction packet here
            if (!LocalPlayer.IsMasterClient && !FactoryManager.EventFromServer && !FactoryManager.EventFromClient)
            {
                LocalPlayer.SendPacket(new DestructEntityRequest(__instance.player.planetId, objId, LocalPlayer.PlayerId));
            }
            else if(!LocalPlayer.IsMasterClient && FactoryManager.EventFromServer && !FactoryManager.EventFromClient && FactoryManager.TargetPlanet == __instance.planet.id && __instance.pathTool.ObjectIsBelt(objId))
            {
                LocalPlayer.SendPacket(new DestructEntityRequest(__instance.player.planetId, objId, LocalPlayer.PlayerId));
            }

            return LocalPlayer.IsMasterClient || FactoryManager.EventFromServer;
        }
    }
}
