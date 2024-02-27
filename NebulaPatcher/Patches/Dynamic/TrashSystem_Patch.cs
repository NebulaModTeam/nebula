#region

using HarmonyLib;
using NebulaModel.Packets.Trash;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(TrashSystem))]
internal class TrashSystem_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(TrashSystem.AddTrash))]
    public static void AddTrash_Prefix(int objId)
    {
        if (!Multiplayer.IsActive) return;

        if (GameMain.localPlanet?.factory != null && objId != 0)
        {
            Multiplayer.Session.Trashes.PlanetId = GameMain.localPlanet.id;
            if (objId != 0)
            {
                var factory = GameMain.localPlanet.factory;
                Multiplayer.Session.Trashes.LocalPos = objId > 0 ? factory.entityPool[objId].pos : factory.prebuildPool[-objId].pos;
            }
        }
        else
        {
            Multiplayer.Session.Trashes.PlanetId = 0;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(TrashSystem.AddTrashFromGroundEnemy))]
    public static bool AddTrashSuppression_Prefix(PlanetFactory factory, int enemyId)
    {
        if (!Multiplayer.IsActive) return true;
        if (Multiplayer.Session.IsClient) return false;

        if (factory != null && enemyId > 0)
        {
            Multiplayer.Session.Trashes.PlanetId = factory.planetId;
            ref var ptr = ref factory.enemyPool[enemyId];
            var height = SkillSystem.RoughHeightByModelIndex[ptr.modelIndex];
            Multiplayer.Session.Trashes.LocalPos = ptr.pos + ptr.pos.normalized * (double)(height * 0.6f);
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(TrashSystem.AddTrashOnPlanet))]
    public static bool AddTrashOnPlanet_Prefix(PlanetData planet, int objId)
    {
        if (!Multiplayer.IsActive) return true;
        if (Multiplayer.Session.IsClient) return false;

        if (planet?.factory != null && objId != 0)
        {
            Multiplayer.Session.Trashes.PlanetId = planet.id;
            Multiplayer.Session.Trashes.LocalPos = objId > 0 ? planet.factory.entityPool[objId].pos : planet.factory.prebuildPool[-objId].pos;
        }
        else
        {
            Multiplayer.Session.Trashes.PlanetId = 0;
        }
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(TrashSystem.ClearAllTrash))]
    public static void ClearAllTrash_Postfix()
    {
        if (!Multiplayer.IsActive) return;

        //Send notification, that somebody clicked on "ClearAllTrash"
        if (!Multiplayer.Session.Trashes.IsIncomingRequest)
        {
            Multiplayer.Session.Network.SendPacket(new TrashSystemClearAllTrashPacket());
        }
        Multiplayer.Session.Trashes.ClientTrashCount = 0;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(TrashSystem.trashCount), MethodType.Getter)]
    public static void Get_trashCount_Postfix(ref int __result)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer) return;

        //Overwrite with our own value since trashRecycleCursor is not reliable in client
        __result = Multiplayer.Session.Trashes.ClientTrashCount;
    }
}
