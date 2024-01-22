#region

using HarmonyLib;
using NebulaModel.Packets.Combat.Mecha;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(PlayerAction_Combat))]
internal class PlayerAction_Combat_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerAction_Combat.ShootTarget))]
    public static void ShootTarget_Postfix(PlayerAction_Combat __instance, EAmmoType ammoType, in SkillTarget target, bool __result)
    {
        NebulaModel.Logger.Log.Debug($"{ammoType} {target.id} {__result}");
        if (!__result || !Multiplayer.IsActive || Multiplayer.Session.Combat.IsIncomingRequest.Value)
        {
            return;
        }

        var isLocal = target.astroId == __instance.localAstroId;
        var ammoItemId = __instance.mecha.ammoItemId;
        var packet = new MechaShootPacket(Multiplayer.Session.LocalPlayer.Id,
            (byte)ammoType, ammoItemId, target.astroId, target.id);

        if (isLocal)
        {
            Multiplayer.Session.Network.SendPacketToLocalPlanet(packet);
        }
        else
        {
            // Currently mecha weapon can not cross star system, so broadcast only to local star
            Multiplayer.Session.Network.SendPacketToLocalStar(packet);
        }
    }
}
