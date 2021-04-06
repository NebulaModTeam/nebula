using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Factory;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PlanetFactory))]
    class BuildFinally_patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("AddPrebuildDataWithComponents")]
        public static bool AddPrebuildDataWithComponents_Prefix(PlanetFactory __instance, PrebuildData prebuild)
        {
            if (!SimulatedWorld.Initialized)
                return true;

            // If the host game called the method, we need to compute the PrebuildId ourself
            if (LocalPlayer.IsMasterClient && !FactoryManager.EventFromClient)
            {
                int nextPrebuildId = FactoryManager.GetNextPrebuildId(__instance);
                FactoryManager.SetPrebuildRequest(__instance.planetId, nextPrebuildId, LocalPlayer.PlayerId);
            }

            // If we are the host we need to notify all the clients to do the same in their game
            // Or if the method was called by the game on a client, we need to send a request to the host to let the host create the object first. 
            if (LocalPlayer.IsMasterClient || !FactoryManager.EventFromServer)
            {
                LocalPlayer.SendPacket(new AddEntityPreviewRequest(__instance.planetId, prebuild));
            }

            // Perform the game code only if you are the host or a client which received a host request to do this action.
            return LocalPlayer.IsMasterClient || FactoryManager.EventFromServer;
        }

        [HarmonyPrefix]
        [HarmonyPatch("BuildFinally")]
        public static bool BuildFinally_Prefix(PlanetFactory __instance, Player player, int prebuildId)
        {
            if (!SimulatedWorld.Initialized)
                return true;

            // If we are executing this on the host, make sure that we were having a corresponding prebuild request
            if (LocalPlayer.IsMasterClient)
            {
                if (!FactoryManager.ContainsPrebuildRequest(__instance.planetId, prebuildId))
                {
                    Log.Warn($"BuildFinally was called without having a corresponding PrebuildRequest for the prebuild {prebuildId} on the planet {__instance.planetId}");
                    return false;
                }

                // Remove the prebuild request from the list since we will now convert it to a real building
                FactoryManager.RemovePrebuildRequest(__instance.planetId, prebuildId);
            }

            // If we are the host we need to notify all the clients to do the same in their game
            // Or if the method was called by the game on a client, we need to send a request to the host to let the host decide if we can create it or not.
            if (LocalPlayer.IsMasterClient || !FactoryManager.EventFromServer)
            {
                LocalPlayer.SendPacket(new BuildEntityRequest(__instance.planetId, prebuildId));
            }

            // Perform the game code only if you are the host or a client which received a host request to do this action.
            // TODO: Look at doing this in a transpiler
            return LocalPlayer.IsMasterClient || FactoryManager.EventFromServer;
        }


        [HarmonyPrefix]
        [HarmonyPatch("DestructFinally")]
        public static bool DestructFinally_Prefix(PlanetFactory __instance, Player player, int objId, ref int protoId)
        {
            if (!SimulatedWorld.Initialized)
                return true;

            // TODO: handle if 2 clients or if host and client trigger a destruct of the same object at the same time

            // If the object is a prebuild, remove it from the prebuild request list
            if (LocalPlayer.IsMasterClient && objId < 0)
            {
                if (!FactoryManager.ContainsPrebuildRequest(__instance.planetId, -objId))
                {
                    Log.Warn($"DestructFinally was called without having a corresponding PrebuildRequest for the prebuild {-objId} on the planet {__instance.planetId}");
                    return false;
                }

                FactoryManager.RemovePrebuildRequest(__instance.planetId, -objId);
            }


            // If we are the host we need to notify all the clients to do the same in their game
            // Or if the method was called by the game on a client, we need to send a request to the host to let the host decide if we can create it or not.
            if (LocalPlayer.IsMasterClient || !FactoryManager.EventFromServer)
            {
                LocalPlayer.SendPacket(new DestructEntityRequest(__instance.planetId, objId));
            }

            // Perform the game code only if you are the host or a client which received a host request to do this action.
            return LocalPlayer.IsMasterClient || FactoryManager.EventFromServer;
        }

        [HarmonyPrefix]
        [HarmonyPatch("UpgradeFinally")]
        public static bool UpgradeFinally_Prefix(PlanetFactory __instance,  Player player, int objId, ItemProto replace_item_proto)
        {
            if (!SimulatedWorld.Initialized)
                return true;

            // If we are the host we need to notify all the clients to do the same in their game
            // Or if the method was called by the game on a client, we need to send a request to the host to let the host decide if we can create it or not.
            if (LocalPlayer.IsMasterClient || !FactoryManager.EventFromServer)
            {
                LocalPlayer.SendPacket(new UpgradeEntityRequest(__instance.planetId, objId, replace_item_proto.ID));
            }

            // Perform the game code only if you are the host or a client which received a host request to do this action.
            return LocalPlayer.IsMasterClient || FactoryManager.EventFromServer;
        }
    }
}
