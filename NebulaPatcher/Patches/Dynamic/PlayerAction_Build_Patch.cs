using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Factory;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PlayerAction_Build))]
    class PlayerAction_Build_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerAction_Build.DoDismantleObject))]
        public static bool DoDismantleObject_Prefix(PlayerAction_Build __instance, int objId)
        {
            if (!SimulatedWorld.Initialized)
            {
                return true;
            }

            int planetId = FactoryManager.TargetPlanet != FactoryManager.PLANET_NONE ? FactoryManager.TargetPlanet : __instance.planet?.id ?? -1;
            // TODO: handle if 2 clients or if host and client trigger a destruct of the same object at the same time

            // If the object is a prebuild, remove it from the prebuild request list
            if (LocalPlayer.IsMasterClient && objId < 0)
            {
                if (!FactoryManager.ContainsPrebuildRequest(planetId, -objId))
                {
                    Log.Warn($"DestructFinally was called without having a corresponding PrebuildRequest for the prebuild {-objId} on the planet {planetId}");
                    return false;
                }

                FactoryManager.RemovePrebuildRequest(planetId, -objId);
            }


            if (LocalPlayer.IsMasterClient || !FactoryManager.EventFromServer)
            {
                LocalPlayer.SendPacket(new DestructEntityRequest(planetId, objId, FactoryManager.PacketAuthor == -1 ? LocalPlayer.PlayerId : FactoryManager.PacketAuthor));
            }

            return LocalPlayer.IsMasterClient || FactoryManager.EventFromServer;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerAction_Build.SetFactoryReferences))]
        public static bool SetFactoryReferences_Prefix()
        {
            if (!SimulatedWorld.Initialized)
            {
                return true;
            }

            if ((FactoryManager.EventFromServer || FactoryManager.EventFromClient) && FactoryManager.PacketAuthor != LocalPlayer.PlayerId && FactoryManager.TargetPlanet != GameMain.localPlanet?.id)
            {
                return false;
            }

            return true;
        }
    }
}
