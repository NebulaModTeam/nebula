#region

using HarmonyLib;
using NebulaModel.Packets.Factory.Laboratory;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UILabWindow))]
internal class UILabWindow_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UILabWindow.OnItemButtonClick))]
    public static void OnItemButtonClick_Prefix(UILabWindow __instance, int index, ref bool __state)
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }

        __state = false;
        var labComponent = GameMain.localPlanet.factory.factorySystem.labPool[__instance.labId];
        if (labComponent is not { researchMode: false, matrixMode: false })
        {
            return;
        }
        //Notify about changing matrix selection
        Multiplayer.Session.Network.SendPacketToLocalStar(
            new LaboratoryUpdateEventPacket(index, __instance.labId, GameMain.localPlanet?.id ?? -1));
        __state = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UILabWindow.OnItemButtonClick))]
    public static void OnItemButtonClick_Postfix(UILabWindow __instance, int index, bool __state)
    {
        // Skip if lab was in neither researchMode nor matrixMode
        if (!Multiplayer.IsActive || __state)
        {
            return;
        }

        var labComponent = GameMain.localPlanet.factory.factorySystem.labPool[__instance.labId];
        if (labComponent.researchMode)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new LaboratoryUpdateCubesPacket(labComponent.matrixServed[index],
                labComponent.matrixIncServed[index], index, __instance.labId, GameMain.localPlanet?.id ?? -1));
        }
        else if (labComponent.matrixMode)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new LaboratoryUpdateStoragePacket(labComponent.served[index],
                labComponent.incServed[index], index, __instance.labId, GameMain.localPlanet?.id ?? -1));
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UILabWindow.OnProductButtonClick))]
    public static void OnItemButtonClick_Prefix(UILabWindow __instance)
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }

        var labComponent = GameMain.localPlanet.factory.factorySystem.labPool[__instance.labId];
        if (labComponent.matrixMode)
        {
            //Notify about withdrawing produced cubes
            Multiplayer.Session.Network.SendPacketToLocalStar(
                new LaboratoryUpdateEventPacket(-3, __instance.labId, GameMain.localPlanet?.id ?? -1));
        }
        else if (!labComponent.researchMode)
        {
            //Notify about selection of research mode
            Multiplayer.Session.Network.SendPacketToLocalStar(
                new LaboratoryUpdateEventPacket(-1, __instance.labId, GameMain.localPlanet?.id ?? -1));
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UILabWindow.OnBackButtonClick))]
    public static void OnBackButtonClick_Prefix(UILabWindow __instance)
    {
        //Notify about recipe reset
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(
                new LaboratoryUpdateEventPacket(-2, __instance.labId, GameMain.localPlanet?.id ?? -1));
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UILabWindow.OnIncSwitchClick))]
    public static void OnIncSwitchClick_Prefix(UILabWindow __instance)
    {
        //Notify about production mode switch
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(
                new LaboratoryUpdateEventPacket(-4, __instance.labId, GameMain.localPlanet?.id ?? -1));
        }
    }
}
