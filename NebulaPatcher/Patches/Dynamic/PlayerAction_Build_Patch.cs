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
            else if ((LocalPlayer.IsMasterClient && FactoryManager.TargetPlanet == GameMain.localPlanet?.id) || !FactoryManager.EventFromServer)
            {
                LocalPlayer.SendPacket(new DestructEntityRequest(FactoryManager.TargetPlanet == FactoryManager.PLANET_NONE ? __instance.planet.id : FactoryManager.TargetPlanet, objId, FactoryManager.PacketAuthor == -1 ? LocalPlayer.PlayerId : FactoryManager.PacketAuthor));
            }

            return LocalPlayer.IsMasterClient || FactoryManager.EventFromServer;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerAction_Build.SetFactoryReferences))]
        public static bool SetFactoryReferences_Prefix()
        {
            if(FactoryManager.EventFromServer || FactoryManager.EventFromClient)
            {
                return false;
            }
            return true;
        }
    }
}
