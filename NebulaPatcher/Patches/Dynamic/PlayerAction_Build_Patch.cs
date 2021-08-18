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
            if (!SimulatedWorld.Instance.Initialized)
            {
                return true;
            }

            int planetId = FactoryManager.Instance.TargetPlanet != FactoryManager.Instance.PLANET_NONE ? FactoryManager.Instance.TargetPlanet : __instance.planet?.id ?? -1;
            // TODO: handle if 2 clients or if host and client trigger a destruct of the same object at the same time

            // If the object is a prebuild, remove it from the prebuild request list
            if (LocalPlayer.Instance.IsMasterClient && objId < 0)
            {
                if (!FactoryManager.Instance.ContainsPrebuildRequest(planetId, -objId))
                {
                    Log.Warn($"DestructFinally was called without having a corresponding PrebuildRequest for the prebuild {-objId} on the planet {planetId}");
                    return false;
                }

                FactoryManager.Instance.RemovePrebuildRequest(planetId, -objId);
            }


            if (LocalPlayer.Instance.IsMasterClient || !FactoryManager.Instance.IsIncomingRequest.Value)
            {
                LocalPlayer.Instance.SendPacket(new DestructEntityRequest(planetId, objId, FactoryManager.Instance.PacketAuthor == FactoryManager.Instance.AUTHOR_NONE ? LocalPlayer.Instance.PlayerId : FactoryManager.Instance.PacketAuthor));
            }

            return LocalPlayer.Instance.IsMasterClient || FactoryManager.Instance.IsIncomingRequest.Value;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerAction_Build.SetFactoryReferences))]
        public static bool SetFactoryReferences_Prefix()
        {
            if (!SimulatedWorld.Instance.Initialized)
            {
                return true;
            }

            if (FactoryManager.Instance.IsIncomingRequest.Value && FactoryManager.Instance.PacketAuthor != LocalPlayer.Instance.PlayerId && FactoryManager.Instance.TargetPlanet != GameMain.localPlanet?.id)
            {
                return false;
            }

            return true;
        }
    }
}
