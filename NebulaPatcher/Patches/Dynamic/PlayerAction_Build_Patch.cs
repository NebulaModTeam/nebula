using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Factory;
using NebulaWorld;
using NebulaWorld.Factory;
using System.Collections.Generic;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PlayerAction_Build))]
    class PlayerAction_Build_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("CreatePrebuilds")]
        public static bool CreatePrebuilds_Prefix(PlayerAction_Build __instance)
        {
            if (__instance.waitConfirm && VFInput._buildConfirm.onDown && __instance.buildPreviews.Count > 0)
            {
                if (!SimulatedWorld.Initialized)
                    return true;

                // Host will just broadcast event to other players
                if (LocalPlayer.IsMasterClient)
                {
                    int planetId = FactoryManager.EventFactory?.planetId ?? GameMain.localPlanet?.id ?? -1;
                    LocalPlayer.SendPacket(new CreatePrebuildsRequest(planetId, __instance.buildPreviews, __instance.previewPose, FactoryManager.PacketAuthor == -1 ? LocalPlayer.PlayerId : FactoryManager.PacketAuthor));
                }

                //If client builds, he need to first send request to the host and wait for reply
                if (!LocalPlayer.IsMasterClient && !FactoryManager.EventFromServer)
                {
                    //Check what client can build from his inventory
                    List<BuildPreview> canBuild = new List<BuildPreview>();
                    //Remove required items from the player's inventory and build only what client can
                    foreach (BuildPreview buildPreview in __instance.buildPreviews)
                    {
                        //This code with flag was taken from original method:
                        bool flag = true;
                        if (buildPreview.coverObjId == 0 || buildPreview.willCover)
                        {
                            int id = buildPreview.item.ID;
                            int num = 1;
                            if (__instance.player.inhandItemId == id && __instance.player.inhandItemCount > 0)
                            {
                                __instance.player.UseHandItems(1);
                            }
                            else
                            {
                                __instance.player.package.TakeTailItems(ref id, ref num, false);
                            }
                            flag = (num == 1);
                            if (flag)
                            {
                                //Give item back to player inventory and wait for the response from the server
                                __instance.player.package.AddItem(id, num);
                            }
                        }
                        if (flag)
                        {
                            canBuild.Add(buildPreview);
                        }
                    }

                    LocalPlayer.SendPacket(new CreatePrebuildsRequest(GameMain.localPlanet?.id ?? -1, canBuild, __instance.previewPose, FactoryManager.PacketAuthor == -1 ? LocalPlayer.PlayerId : FactoryManager.PacketAuthor));
                    return false;
                }
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("DoDestructObject")]
        public static bool DoDestructObject_Prefix(PlayerAction_Build __instance, int objId)
        {
            if (!SimulatedWorld.Initialized)
                return true;

            //Clients needs to send destruction packet here
            if (!LocalPlayer.IsMasterClient && !FactoryManager.EventFromServer && !FactoryManager.EventFromClient)
            {
                LocalPlayer.SendPacket(new DestructEntityRequest(__instance.player.planetId, objId, LocalPlayer.PlayerId));
            }

            return LocalPlayer.IsMasterClient || FactoryManager.EventFromServer;
        }
      
        [HarmonyPrefix]
        [HarmonyPatch("AfterPrebuild")]
        public static bool AfterPrebuild_Prefix()
        {
            return !FactoryManager.EventFromServer && !FactoryManager.EventFromClient;
        }
    }
}
