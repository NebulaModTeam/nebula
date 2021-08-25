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
            if (!Multiplayer.IsActive)
            {
                return true;
            }

            int planetId = Multiplayer.Session.Factories.TargetPlanet != FactoryManager.PLANET_NONE ? Multiplayer.Session.Factories.TargetPlanet : __instance.planet?.id ?? -1;
            // TODO: handle if 2 clients or if host and client trigger a destruct of the same object at the same time

            // If the object is a prebuild, remove it from the prebuild request list
            if (LocalPlayer.IsMasterClient && objId < 0)
            {
                if (!Multiplayer.Session.Factories.ContainsPrebuildRequest(planetId, -objId))
                {
                    Log.Warn($"DestructFinally was called without having a corresponding PrebuildRequest for the prebuild {-objId} on the planet {planetId}");
                    return false;
                }

                Multiplayer.Session.Factories.RemovePrebuildRequest(planetId, -objId);
            }


            if (LocalPlayer.IsMasterClient || !Multiplayer.Session.Factories.IsIncomingRequest)
            {
                LocalPlayer.SendPacket(new DestructEntityRequest(planetId, objId, Multiplayer.Session.Factories.PacketAuthor == FactoryManager.AUTHOR_NONE ? LocalPlayer.PlayerId : Multiplayer.Session.Factories.PacketAuthor));
            }

            return LocalPlayer.IsMasterClient || Multiplayer.Session.Factories.IsIncomingRequest;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerAction_Build.SetFactoryReferences))]
        public static bool SetFactoryReferences_Prefix()
        {
            if (!Multiplayer.IsActive)
            {
                return true;
            }

            if (Multiplayer.Session.Factories.IsIncomingRequest && Multiplayer.Session.Factories.PacketAuthor != LocalPlayer.PlayerId && Multiplayer.Session.Factories.TargetPlanet != GameMain.localPlanet?.id)
            {
                return false;
            }

            return true;
        }
    }
}
