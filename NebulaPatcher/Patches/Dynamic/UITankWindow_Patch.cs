#region

using HarmonyLib;
using NebulaModel.Packets.Factory.Tank;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UITankWindow))]
internal class UITankWindow_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UITankWindow.OnTakeBackPointerUp))]
    public static void OnTakeBackPointerUp_Postfix(UITankWindow __instance)
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }
        var thisTank = __instance.storage.tankPool[__instance.tankId];
        Multiplayer.Session.Network.SendPacketToLocalStar(new TankStorageUpdatePacket(in thisTank,
            GameMain.localPlanet?.id ?? -1));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UITankWindow.OnOutputSwitchClick))]
    public static void OnOutputSwitchClick_Postfix(UITankWindow __instance)
    {
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new TankInputOutputSwitchPacket(__instance.tankId, false,
                __instance.storage.tankPool[__instance.tankId].outputSwitch, GameMain.localPlanet?.id ?? -1));
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UITankWindow.OnInputSwitchClick))]
    public static void OnInputSwitchClick_Postfix(UITankWindow __instance)
    {
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Network.SendPacketToLocalStar(new TankInputOutputSwitchPacket(__instance.tankId, true,
                __instance.storage.tankPool[__instance.tankId].inputSwitch, GameMain.localPlanet?.id ?? -1));
        }
    }
}
