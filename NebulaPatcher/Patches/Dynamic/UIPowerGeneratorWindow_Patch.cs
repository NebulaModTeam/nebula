#region

using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Factory.PowerGenerator;
using NebulaModel.Packets.Factory.RayReceiver;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIPowerGeneratorWindow))]
internal class UIPowerGeneratorWindow_Patch
{
    private static bool boost;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIPowerGeneratorWindow.OnGammaMode1Click))]
    public static void OnGammaMode1Click_Postfix(UIPowerGeneratorWindow __instance)
    {
        //Notify about change of ray receiver to mode "electricity"
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new RayReceiverChangeModePacket(__instance.generatorId,
                RayReceiverMode.Electricity, GameMain.localPlanet?.id ?? -1));
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIPowerGeneratorWindow.OnGammaMode2Click))]
    public static void OnGammaMode2Click_Postfix(UIPowerGeneratorWindow __instance)
    {
        //Notify about change of ray receiver to mode "produce photons"
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new RayReceiverChangeModePacket(__instance.generatorId,
                RayReceiverMode.Photon, GameMain.localPlanet?.id ?? -1));
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIPowerGeneratorWindow.OnCataButtonClick))]
    public static void OnCataButtonClick_Postfix(UIPowerGeneratorWindow __instance)
    {
        //Notify about changing amount of gravitational lens
        if (!Multiplayer.IsActive)
        {
            return;
        }
        var packet = new RayReceiverChangeLensPacket(__instance.generatorId,
            __instance.powerSystem.genPool[__instance.generatorId].catalystPoint,
            __instance.powerSystem.genPool[__instance.generatorId].catalystIncPoint,
            GameMain.localPlanet?.id ?? -1);
        Multiplayer.Session.Network.SendPacketToLocalStar(packet);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIPowerGeneratorWindow.OnFuelButtonClick))]
    public static void OnFuelButtonClick_Postfix(UIPowerGeneratorWindow __instance)
    {
        //Notify about changing amount of fuel in power plant
        if (!Multiplayer.IsActive)
        {
            return;
        }
        var thisComponent = __instance.powerSystem.genPool[__instance.generatorId];
        Multiplayer.Session.Network.SendPacketToLocalStar(new PowerGeneratorFuelUpdatePacket(__instance.generatorId,
            thisComponent.fuelId, thisComponent.fuelCount, thisComponent.fuelInc, GameMain.localPlanet?.id ?? -1));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIPowerGeneratorWindow.OnProductButtonClick))]
    public static void OnProductButtonClick(UIPowerGeneratorWindow __instance)
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }
        var thisComponent = __instance.powerSystem.genPool[__instance.generatorId];
        Multiplayer.Session.Network.SendPacketToLocalStar(
            new PowerGeneratorProductUpdatePacket(thisComponent, __instance.factory.planetId));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIPowerGeneratorWindow.OnGeneratorIdChange))]
    public static void OnGeneratorIdChange_Postfix(UIPowerGeneratorWindow __instance)
    {
        if (Multiplayer.IsActive && __instance.active)
        {
            boost = __instance.boostSwitch.isOn;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIPowerGeneratorWindow._OnUpdate))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnUpdate_Prefix(UIPowerGeneratorWindow __instance)
    {
        //Notify about boost change in sandbox mode
        if (!Multiplayer.IsActive || boost == __instance.boostSwitch.isOn)
        {
            return;
        }
        boost = __instance.boostSwitch.isOn;
        Multiplayer.Session.Network.SendPacketToLocalStar(new EntityBoostSwitchPacket
            (GameMain.localPlanet?.id ?? -1, EBoostEntityType.ArtificialStar, __instance.generatorId, boost));
    }
}
