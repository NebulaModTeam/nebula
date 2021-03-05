using NebulaModel.Logger;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Players;
using System.Collections.Generic;
using UnityEngine;

namespace NebulaWorld
{
    /// <summary>
    /// This class keeps track of our simulated world. It holds all temporary entities like remote player models 
    /// and also helps to execute some remote player actions that you would want to replicate on the local client.
    /// </summary>
    public static class SimulatedWorld
    {
        // TODO: Keep track here of any additional multiplayer stuff that gets created that is not really there in the single player game.
        static Dictionary<ushort, RemotePlayerModel> remotePlayersModels;

        public static void Initialize()
        {
            remotePlayersModels = new Dictionary<ushort, RemotePlayerModel>();
        }

        public static void SpawnRemotePlayerModel(ushort playerId)
        {
            RemotePlayerModel model = new RemotePlayerModel(playerId);
            remotePlayersModels.Add(playerId, model);

            Log.Error($"Spawning a player model for the player ID: {playerId}");
        }

        public static void DestroyRemotePlayerModel(ushort playerId)
        {
            if (remotePlayersModels.TryGetValue(playerId, out RemotePlayerModel player))
            {
                player.Destroy();
                remotePlayersModels.Remove(playerId);
            }
        }

        public static void UpdateRemotePlayerPosition(PlayerMovement packet)
        {
            if (remotePlayersModels.TryGetValue(packet.PlayerId, out RemotePlayerModel player))
            {
                player.Movement.UpdatePosition(packet);
            }
            else
            {
                Log.Error($"No player found with ID: {packet.PlayerId}");
            }
        }

        public static void UpdateRemotePlayerAnimation(PlayerAnimationUpdate packet)
        {
            if (remotePlayersModels.TryGetValue(packet.PlayerId, out RemotePlayerModel player))
            {
                player.Animator.UpdateState(packet);
            }
        }

        public static void UpdatePlayerColor(ushort playerId, Color color)
        {
            RemotePlayerModel player;
            if (!remotePlayersModels.TryGetValue(playerId, out player))
                return;

            // Apply new color to each part of the mecha
            Renderer[] componentsInChildren = player.PlayerTransform.gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
            foreach (Renderer r in componentsInChildren)
            {
                if (r.material?.name == "icarus-armor (Instance)")
                {
                    r.material.SetColor("_Color", color);
                }
            }
        }

        public static void MineVegetable(VegeMined packet)
        {
            PlanetData planet = GameMain.galaxy?.PlanetById(packet.PlanetID);
            if (planet == null)
                return;

            if (packet.isVegetable) // Trees, rocks, leaves, etc
            {
                VegeData vData = (VegeData)planet.factory?.GetVegeData(packet.MiningID);
                VegeProto vProto = LDB.veges.Select((int)vData.protoId);
                if (vProto != null && planet.id == GameMain.localPlanet?.id)
                {
                    VFEffectEmitter.Emit(vProto.MiningEffect, vData.pos, vData.rot);
                    VFAudio.Create(vProto.MiningAudio, null, vData.pos, true);
                }
                planet.factory?.RemoveVegeWithComponents(vData.id);
            }
            else // veins
            {
                VeinData vData = (VeinData)planet.factory?.GetVeinData(packet.MiningID);
                VeinProto vProto = LDB.veins.Select((int)vData.type);
                if (vProto != null)
                {
                    if (planet.factory?.veinPool[packet.MiningID].amount > 0)
                    {
                        VeinData[] vPool = planet.factory?.veinPool;
                        PlanetData.VeinGroup[] vGroups = planet.factory?.planet.veinGroups;
                        long[] vAmounts = planet.veinAmounts;
                        vPool[packet.MiningID].amount -= 1;
                        vGroups[(int)vData.groupIndex].amount -= 1;
                        vAmounts[(int)vData.type] -= 1;

                        if (planet.id == GameMain.localPlanet?.id)
                        {
                            VFEffectEmitter.Emit(vProto.MiningEffect, vData.pos, Maths.SphericalRotation(vData.pos, 0f));
                            VFAudio.Create(vProto.MiningAudio, null, vData.pos, true);
                        }
                    }
                    else
                    {
                        PlanetData.VeinGroup[] vGroups = planet.factory?.planet.veinGroups;
                        vGroups[vData.groupIndex].count -= 1;

                        if (planet.id == GameMain.localPlanet?.id)
                        {
                            VFEffectEmitter.Emit(vProto.MiningEffect, vData.pos, Maths.SphericalRotation(vData.pos, 0f));
                            VFAudio.Create(vProto.MiningAudio, null, vData.pos, true);
                        }

                        planet.factory?.RemoveVeinWithComponents(vData.id);
                    }
                }
            }
        }

    }
}
