using HarmonyLib;
using NebulaWorld;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GalacticTransport))]
    class GalacticTransport_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("AddStationComponent")]
        public static bool AddStationComponent_Prefix(GalacticTransport __instance, int planetId, StationComponent station)
        {
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return true;
            }
            // if we are a client and have a fake station already saved, update it with the full data given now
            // this should happen when this client arrives at a planet for the first time whichs interplanetar logistic
            // he was already syncing
            for(int i = 0; i < GameMain.data.galacticTransport.stationPool.Length; i++)
            {
                if(GameMain.data.galacticTransport.stationPool[i]?.gid == station.gid)
                {
                    GameMain.data.galacticTransport.RemoveStationComponent(i);
                    return true;
                }
            }
            // if we are a client and have no fake station then just add the new one normally
            // this should happen when a player places an ILS on a FactoryData known to this client
            // or when this client arrives at a planet for the first time which contains ILS
            return true;
        }

        /*
         * We probably dont need to patch this as EntityManager.cs will at some point keep track of removed entities.
         * And as such will be able to call this for clients if needed
        [HarmonyPrefix]
        [HarmonyPatch("RemoveStationComponent")]
        public static bool RemoveStationComponent_Prefix(GalacticTransport __instance, int gid)
        {
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return true;
            }
            
            return false;
        }
        */
    }
}
