using HarmonyLib;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Trash;
using NebulaWorld.Factory;
using NebulaWorld.Logistics;
using NebulaWorld.MonoBehaviours.Remote;
using NebulaWorld.Trash;
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
        public static bool IsGameLoaded { get; private set; }
        public static bool IsPlayerJoining { get; set; }

        public static void Initialize()
        {
            FactoryManager.Initialize();
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
            IsGameLoaded = false;
            IsPlayerJoining = false;
        }

        public static void OnPlayerJoining()
        {
            if (!IsPlayerJoining)
            {
                IsPlayerJoining = true;
                GameMain.isFullscreenPaused = true;
                InGamePopup.ShowInfo("Loading", "Player joining the game, please wait", null);
            }
        }

        public static void OnAllPlayersSyncCompleted()
        {
            IsPlayerJoining = false;
            InGamePopup.FadeOut();
            GameMain.isFullscreenPaused = false;
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
            if (!remotePlayersModels.ContainsKey(playerData.PlayerId))
            {
                RemotePlayerModel model = new RemotePlayerModel(playerData.PlayerId);
                remotePlayersModels.Add(playerData.PlayerId, model);
            }

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
                player.Effects.UpdateState(packet);
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

        public static void OnILSShipUpdate(ILSShipData packet)
        {
            if (packet.IdleToWork)
            {
                ILSShipManager.IdleShipGetToWork(packet);
            }
            else
            {
                ILSShipManager.WorkShipBackToIdle(packet);
            }
        }

        public static void OnILSShipItemsUpdate(ILSShipItems packet)
        {
            ILSShipManager.AddTakeItem(packet);
        }

        public static void OnStationUIChange(StationUI packet)
        {
            StationUIManager.UpdateUI(packet);
        }

        public static void OnILSRemoteOrderUpdate(ILSRemoteOrderData packet)
        {
            ILSShipManager.UpdateRemoteOrder(packet);
        }

        public static void MineVegetable(VegeMined packet)
        {
            if (!remotePlayersModels.TryGetValue((ushort)packet.PlayerId, out RemotePlayerModel pModel))
            {
                Debug.Log("FAILED TO SYNC VEGE DATA");
            }
            PlanetData planet = GameMain.galaxy?.PlanetById(pModel.Movement.localPlanetId);
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

        public static int GenerateTrashOnPlayer(TrashSystemNewTrashCreatedPacket packet)
        {
            if (remotePlayersModels.TryGetValue(packet.PlayerId, out RemotePlayerModel player))
            {
                TrashData trashData = packet.GetTrashData();
                //Calculate trash position based on the current player's model position
                RemotePlayerMovement.Snapshot lastPosition = player.Movement.GetLastPosition();
                trashData.uPos = new VectorLF3(lastPosition.UPosition.x, lastPosition.UPosition.y, lastPosition.UPosition.z);

                TrashManager.NewTrashFromOtherPlayers = true;
                int myId = GameMain.data.trashSystem.container.NewTrash(packet.GetTrashObject(), trashData);
                TrashManager.NewTrashFromOtherPlayers = false;

                return myId;
            }
            return 0;
        }

        public static void OnGameLoadCompleted()
        {
            if (Initialized == false)
                return;

            Log.Info("Game has finished loading");

            // Assign our own color
            UpdatePlayerColor(LocalPlayer.PlayerId, LocalPlayer.Data.Color);

            // Change player location from spawn to the last known
            VectorLF3 UPosition = new VectorLF3(LocalPlayer.Data.UPosition.x, LocalPlayer.Data.UPosition.y, LocalPlayer.Data.UPosition.z);
            if (UPosition != VectorLF3.zero)
            {
                GameMain.mainPlayer.planetId = LocalPlayer.Data.LocalPlanetId;
                if (LocalPlayer.Data.LocalPlanetId == -1)
                {
                    GameMain.mainPlayer.uPosition = UPosition;
                }
                else
                {
                    GameMain.mainPlayer.position = LocalPlayer.Data.LocalPlanetPosition.ToUnity();
                    GameMain.mainPlayer.uPosition = new VectorLF3(GameMain.localPlanet.uPosition.x + GameMain.mainPlayer.position.x, GameMain.localPlanet.uPosition.y + GameMain.mainPlayer.position.y, GameMain.localPlanet.uPosition.z + GameMain.mainPlayer.position.z);
                }
                GameMain.mainPlayer.uRotation = Quaternion.Euler(LocalPlayer.Data.Rotation.ToUnity());

                //Load player's saved data from the last session.
                AccessTools.Property(typeof(Player), "package").SetValue(GameMain.mainPlayer, LocalPlayer.Data.Mecha.Inventory, null);
                GameMain.mainPlayer.mecha.forge = LocalPlayer.Data.Mecha.Forge;
                GameMain.mainPlayer.mecha.coreEnergy = LocalPlayer.Data.Mecha.CoreEnergy;
                GameMain.mainPlayer.mecha.reactorEnergy = LocalPlayer.Data.Mecha.ReactorEnergy;
                GameMain.mainPlayer.mecha.reactorStorage = LocalPlayer.Data.Mecha.ReactorStorage;
                GameMain.mainPlayer.mecha.warpStorage = LocalPlayer.Data.Mecha.WarpStorage;

                //Fix references that brokes during import
                AccessTools.Property(typeof(MechaForge), "mecha").SetValue(GameMain.mainPlayer.mecha.forge, GameMain.mainPlayer.mecha, null);
                AccessTools.Property(typeof(MechaForge), "player").SetValue(GameMain.mainPlayer.mecha.forge, GameMain.mainPlayer, null);
                GameMain.mainPlayer.mecha.forge.gameHistory = GameMain.data.history;

                //Update player's Mecha tech bonuses
                LocalPlayer.Data.Mecha.TechBonuses.UpdateMech(GameMain.mainPlayer.mecha);
            }

            LocalPlayer.SetReady();

            IsGameLoaded = true;
        }

        public static void OnDronesDraw()
        {
            foreach(KeyValuePair<ushort, RemotePlayerModel> remoteModel in remotePlayersModels)
            {
                remoteModel.Value.MechaInstance.droneRenderer.Draw();
            }
        }

        public static void OnDronesGameTick(long time, float dt)
        {
            double tmp = 0;
            double tmp2 = 0;
            //Update drones positions based on their targets
            foreach (KeyValuePair<ushort, RemotePlayerModel> remoteModel in remotePlayersModels)
            {
                MechaDrone[] drones = remoteModel.Value.PlayerInstance.mecha.drones;
                int droneCount = remoteModel.Value.PlayerInstance.mecha.droneCount;
                for (int i = 0; i < droneCount; i++)
                {
                    if (drones[i].stage != 0)
                    {
                        //To-do: fix the correct factory here
                        //To-do: Optimize getting local position of player
                        drones[i].Update(GameMain.localPlanet.factory.prebuildPool, remoteModel.Value.Movement.GetLastPosition().LocalPlanetPosition.ToUnity(), dt, ref tmp, ref tmp2, 0);
                    }
                }
                remoteModel.Value.MechaInstance.droneRenderer.Update();
            }
        }
    }
}
