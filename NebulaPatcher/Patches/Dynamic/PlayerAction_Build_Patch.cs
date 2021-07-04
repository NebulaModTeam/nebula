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

            // Make sure these are being set
            __instance.SetFactoryReferences();
            __instance.SetToolsFactoryReferences();

            if (LocalPlayer.IsMasterClient || !FactoryManager.EventFromServer)
            {
                LocalPlayer.SendPacket(new DestructEntityRequest(__instance.factory.planetId, objId, FactoryManager.PacketAuthor == -1 ? LocalPlayer.PlayerId : FactoryManager.PacketAuthor));
            }

            return LocalPlayer.IsMasterClient || FactoryManager.EventFromServer;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerAction_Build.SetFactoryReferences))]
        public static bool SetFactoryReferences_Prefix()
        {
            if((FactoryManager.EventFromServer || FactoryManager.EventFromClient) && FactoryManager.PacketAuthor != LocalPlayer.PlayerId)
            {
                return false;
            }
            return true;
        }
    }
}
