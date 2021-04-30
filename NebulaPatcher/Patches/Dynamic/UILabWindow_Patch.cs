using HarmonyLib;
using NebulaModel.Packets.Factory.Laboratory;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UILabWindow))]
    class UILabWindow_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnItemButtonClick")]
        public static void OnItemButtonClick_Prefix(UILabWindow __instance, int index)
        {
            if (!SimulatedWorld.Initialized)
            {
                return;
            }

            LabComponent labComponent = GameMain.localPlanet.factory.factorySystem.labPool[__instance.labId];
            if (labComponent.researchMode)
            {
                if (GameMain.mainPlayer.inhandItemId > 0 && GameMain.mainPlayer.inhandItemCount > 0)
                {
                    //Notify about depositing source cubes
                    ItemProto[] matrixProtos = (ItemProto[])AccessTools.Field(typeof(UILabWindow), "matrixProtos").GetValue(__instance);
                    int id = matrixProtos[index].ID;
                    if (GameMain.mainPlayer.inhandItemId == id)
                    {
                        int num = labComponent.matrixServed[index] / 3600;
                        int num2 = 100 - num;
                        if (num2 < 0)
                        {
                            num2 = 0;
                        }
                        int num3 = (GameMain.mainPlayer.inhandItemCount >= num2) ? num2 : GameMain.mainPlayer.inhandItemCount;
                        if (num3 > 0)
                        {
                            LocalPlayer.SendPacketToLocalStar(new LaboratoryUpdateCubesPacket(labComponent.matrixServed[index] + num3 * 3600, index, __instance.labId, GameMain.localPlanet?.factoryIndex ?? -1));
                        }
                    }
                }
                else
                {
                    //Notify about widthrawing source cubes
                    if ((int)(labComponent.matrixServed[index] / 3600) > 0)
                    {
                        LocalPlayer.SendPacketToLocalStar(new LaboratoryUpdateCubesPacket(0, index, __instance.labId, GameMain.localPlanet?.factoryIndex ?? -1));
                    }
                }
            }
            else if (labComponent.matrixMode)
            {
                if (GameMain.mainPlayer.inhandItemId > 0 && GameMain.mainPlayer.inhandItemCount > 0)
                {
                    //Notify about depositing source items to the center
                    int num7 = labComponent.served[index];
                    int num8 = 100 - num7;
                    if (num8 < 0)
                    {
                        num8 = 0;
                    }
                    int num9 = (GameMain.mainPlayer.inhandItemCount >= num8) ? num8 : GameMain.mainPlayer.inhandItemCount;
                    if (num9 > 0)
                    {
                        LocalPlayer.SendPacketToLocalStar(new LaboratoryUpdateStoragePacket(labComponent.served[index] + num9, index, __instance.labId, GameMain.localPlanet?.factoryIndex ?? -1));
                    }
                }
                else
                {
                    //Notify about withdrawing source items from the center
                    if (labComponent.served[index] > 0)
                    {
                        LocalPlayer.SendPacketToLocalStar(new LaboratoryUpdateStoragePacket(0, index, __instance.labId, GameMain.localPlanet?.factoryIndex ?? -1));
                    }
                }
            }
            else
            {
                //Notify about changing matrix selection
                LocalPlayer.SendPacketToLocalStar(new LaboratoryUpdateEventPacket(index, __instance.labId, GameMain.localPlanet?.factoryIndex ?? -1));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnProductButtonClick")]
        public static void OnItemButtonClick_Prefix(UILabWindow __instance)
        {
            if (!SimulatedWorld.Initialized)
            {
                return;
            }

            LabComponent labComponent = GameMain.localPlanet.factory.factorySystem.labPool[__instance.labId];
            if (labComponent.matrixMode)
            {
                //Notify about withdrawing produced cubes
                LocalPlayer.SendPacketToLocalStar(new LaboratoryUpdateEventPacket(-3, __instance.labId, GameMain.localPlanet?.factoryIndex ?? -1));
            }
            else if (!labComponent.researchMode)
            {
                //Notify about selection of research mode
                LocalPlayer.SendPacketToLocalStar(new LaboratoryUpdateEventPacket(-1, __instance.labId, GameMain.localPlanet?.factoryIndex ?? -1));
            }

        }

        [HarmonyPrefix]
        [HarmonyPatch("OnBackButtonClick")]
        public static void OnBackButtonClick_Prefix(UILabWindow __instance)
        {
            //Notify about recipe reset
            if (SimulatedWorld.Initialized)
            {
                LocalPlayer.SendPacketToLocalStar(new LaboratoryUpdateEventPacket(-2, __instance.labId, GameMain.localPlanet?.factoryIndex ?? -1));
            }
        }
    }
}
