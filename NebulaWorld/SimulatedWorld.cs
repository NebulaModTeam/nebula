using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NebulaWorld
{
    /// <summary>
    /// This class keeps track of our simulated world. It holds all temporary entities like remote player models 
    /// and also helps to execute some remote player actions that you would want to replicate on the local client.
    /// </summary>
    public static class SimulatedWorld
    {
        static Dictionary<ushort, RemotePlayerModel> remotePlayersModels;

        private static bool initialized;

        public static void Initialize()
        {
            remotePlayersModels = new Dictionary<ushort, RemotePlayerModel>();
            initialized = true;
        }

        /// <summary>
        /// Removes any simulated entities that was added to the scene for a game.
        /// </summary>
        public static void Clear()
        {
            foreach (var model in remotePlayersModels.Values)
            {
                model.Destroy();
            }

            remotePlayersModels.Clear();
            initialized = false;
        }

        public static void SpawnRemotePlayerModel(PlayerData playerData)
        {
            RemotePlayerModel model = new RemotePlayerModel(playerData.PlayerId);
            remotePlayersModels.Add(playerData.PlayerId, model);
            UpdatePlayerColor(playerData.PlayerId, playerData.Color);
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
        }

        public static void UpdateRemotePlayerAnimation(PlayerAnimationUpdate packet)
        {
            if (remotePlayersModels.TryGetValue(packet.PlayerId, out RemotePlayerModel player))
            {
                player.Animator.UpdateState(packet);
            }
        }

        public static void UpdatePlayerColor(ushort playerId, Float3 color)
        {
            RemotePlayerModel player;
            if (!remotePlayersModels.TryGetValue(playerId, out player))
                return;
            UpdatePlayerColor(player.PlayerTransform, color);
        }

        public static void UpdatePlayerColor(Transform transform, Float3 color)
        {
            Log.Info($"Changing color of {transform.gameObject.name} to {color}");

            // Apply new color to each part of the mecha
            Renderer[] componentsInChildren = transform.gameObject.GetComponentsInChildren<Renderer>(includeInactive: false);
            foreach (Renderer r in componentsInChildren)
            {
                if (r.material?.name.Contains("icarus-armor") ?? false)
                {
                    r.material.SetColor("_Color", color.ToColor());
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

        public static void OnGameLoadCompleted()
        {
            if (initialized == false)
                return;

            Log.Info("Game has finished loading");
            //Assign our own coler
            UpdatePlayerColor(GameMain.data.mainPlayer.transform,
                new Float3(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f)));

            LocalPlayer.SetReady();
        }
    }
}
