using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Factory;
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
        static Dictionary<ushort, RemotePlayerModel> remotePlayersModels;

        public static bool Initialized { get; private set; }

        public static void Initialize()
        {
            remotePlayersModels = new Dictionary<ushort, RemotePlayerModel>();
            Initialized = true;
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
            Initialized = false;
        }

        public static void UpdateGameState(GameState state)
        {
            // We allow for a small drift of 5 ticks since the tick offset using the ping is only an approximation
            if (GameMain.gameTick > 0 && Mathf.Abs(state.gameTick - GameMain.gameTick) > 5)
            {
                Log.Info($"Game Tick got updated since it was desynced, was {GameMain.gameTick}, received {state.gameTick}");
                GameMain.gameTick = state.gameTick;
            }
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
            Transform transform;
            RemotePlayerModel remotePlayerModel;
            if (playerId == LocalPlayer.PlayerId)
            {
                transform = GameMain.data.mainPlayer.transform;
            }
            else if (remotePlayersModels.TryGetValue(playerId, out remotePlayerModel))
            {
                transform = remotePlayerModel.PlayerTransform;
            }
            else
            {
                Log.Error("Could not find the transform for player with ID " + playerId);
                return;
            }

            Log.Info($"Changing color of player {playerId} to {color}");

            // Apply new color to each part of the mecha
            SkinnedMeshRenderer[] componentsInChildren = transform.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (Renderer r in componentsInChildren)
            {
                if (r.material?.name.StartsWith("icarus-armor") ?? false)
                {
                    r.material.SetColor("_Color", color.ToColor());
                }
            }

            // We changed our own color, so we have to let others know
            if (LocalPlayer.PlayerId == playerId)
            {
                LocalPlayer.SendPacket(new PlayerColorChanged(playerId, color));
            }
        }

        public static void PlaceEntity(EntityPlaced packet)
        {
            int ret = GameMain.mainPlayer.factory.AddEntityDataWithComponents(new EntityData
            {
                protoId = packet.protoId,
                pos = new Vector3(packet.pos.x, packet.pos.y, packet.pos.z),
                rot = new Quaternion(packet.rot.x, packet.rot.y, packet.rot.z, packet.rot.w)
            }, 0);

            GameMain.mainPlayer.controller.actionBuild.NotifyBuilt(-0, ret);
            GameMain.history.MarkItemBuilt((int)packet.protoId);
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
            if (Initialized == false)
                return;

            Log.Info("Game has finished loading");

            // Assign our own color
            UpdatePlayerColor(LocalPlayer.PlayerId, LocalPlayer.Data.Color);

            // TODO: Investigate where are the weird position coming from ?
            // GameMain.mainPlayer.transform.position = data.Position.ToUnity();
            // GameMain.mainPlayer.transform.eulerAngles = data.Rotation.ToUnity();

            LocalPlayer.SetReady();
        }
    }
}
