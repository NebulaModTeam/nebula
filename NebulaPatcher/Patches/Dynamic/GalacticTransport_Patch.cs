using HarmonyLib;
using NebulaWorld;
using NebulaModel.Packets.Logistics;
using NebulaWorld.Logistics;
using System.Collections.Generic;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GalacticTransport))]
    class GalacticTransport_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("SetForNewGame")]
        public static void SetForNewGame_Postfix()
        {
            if(SimulatedWorld.Initialized && !LocalPlayer.IsMasterClient)
            {
                LocalPlayer.SendPacket(new ILSRequestgStationPoolSync());
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("AddStationComponent")]
        public static bool AddStationComponent_Prefix(GalacticTransport __instance, int planetId, StationComponent station)
        {
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient || ILSShipManager.PatchLockILS)
            {
                return true;
            }
            if (!ILSShipManager.AddStationComponentQueue.ContainsKey(planetId))
            {
                ILSShipManager.AddStationComponentQueue.Add(planetId, new List<StationComponent>());
            }
            // use a queue here as we need to wait for the GId sent by server
            ILSShipManager.AddStationComponentQueue[planetId].Add(station);
            LocalPlayer.SendPacket(new ILSAddStationComponentRequest(planetId, station.shipDockPos));

            // if we are a client and have no fake station then just add the new one normally
            // this should happen when a player places an ILS on a FactoryData known to this client
            // or when this client arrives at a planet for the first time which contains ILS

            //TODO: i think this should be false and only added via ILSAddStationComponentResponseProcessor
            // but when client builds a ILS on a remote planet the shipDockPos is 0 and thus the checks fail.
            // all in all there is some investigation needed here
            return true;
        }
    }
}
