using System.Collections.Generic;
using NebulaModel.Packets.Factory;

namespace NebulaWorld.Factory
{
    public class BuildToolManager
    {
        // Is there a better way of determining which BuildTool we are sending these from?
        public static bool CreatePrebuilds(BuildTool __instance)
        {
            if (!SimulatedWorld.Initialized)
                return true;

            // Host will just broadcast event to other players
            if (LocalPlayer.IsMasterClient)
            {
                int planetId = FactoryManager.EventFactory?.planetId ?? GameMain.localPlanet?.id ?? -1;
                LocalPlayer.SendPacketToLocalStar(new CreatePrebuildsRequest(planetId, __instance.buildPreviews, FactoryManager.PacketAuthor == -1 ? LocalPlayer.PlayerId : FactoryManager.PacketAuthor));
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
                    if(__instance.GetType() == typeof(BuildTool_Click) && (buildPreview.condition == EBuildCondition.Ok && buildPreview.coverObjId == 0))
                    {
                        TakeItems(__instance, buildPreview, ref flag);
                    }
                    else if ((__instance.GetType() == typeof(BuildTool_Inserter) || __instance.GetType() == typeof(BuildTool_Path)) && (buildPreview.coverObjId == 0 || buildPreview.willRemoveCover))
                    {
                            TakeItems(__instance, buildPreview, ref flag);
                    }
                    if (flag)
                    {
                        canBuild.Add(buildPreview);
                    }
                }

                LocalPlayer.SendPacket(new CreatePrebuildsRequest(GameMain.localPlanet?.id ?? -1, canBuild, FactoryManager.PacketAuthor == -1 ? LocalPlayer.PlayerId : FactoryManager.PacketAuthor));
                return false;
            }
            return true;
        }

        private static void TakeItems(BuildTool __instance,BuildPreview buildPreview, ref bool flag)
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
    }
}
