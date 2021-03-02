using NebulaClient.GameLogic;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Session;
using UnityEngine;

namespace NebulaClient.MonoBehaviours
{
    public class MultiplayerSession : MonoBehaviour
    {
        public static MultiplayerSession instance;

        public Client Client { get; private set; }
        public PlayerManager PlayerManager { get; private set; }

        private string serverIp;
        private int serverPort;

        void Awake()
        {
            instance = this;

            Client = new Client();
            Client.PacketProcessor.SubscribeReusable<JoinSessionConfirmed>(OnJoinSessionConfirmed);
            Client.PacketProcessor.SubscribeReusable<RemotePlayerJoined>(OnRemotePlayerJoined);
            Client.PacketProcessor.SubscribeReusable<PlayerDisconnected>(OnRemotePlayerDisconnect);
            Client.PacketProcessor.SubscribeReusable<Movement>(OnPlayerMovement);
            Client.PacketProcessor.SubscribeReusable<PlayerAnimationUpdate>(OnPlayerAnimationUpdate);
            Client.PacketProcessor.SubscribeReusable<VegeMined>(OnVegeMined);
        }

        public void Connect(string ip, int port)
        {
            serverIp = ip;
            serverPort = port;
            Client.Connect(ip, port);

            PlayerManager = new PlayerManager();
        }

        public void TryToReconnect()
        {
            Disconnect();
            Connect(serverIp, serverPort);
            // TODO: Should freeze game and add a spinner or something during the reconnection.
            // Else the player can still move around during the reconnection procedure which is weird
        }

        public void Disconnect()
        {
            if (Client.IsConnected)
            {
                Client.Disconnect();
            }

            CleanupSession();

        }

        public void LeaveGame()
        {
            Disconnect();

            // Go back to the main menu
            if (!UIRoot.instance.backToMainMenu)
            {
                UIRoot.instance.backToMainMenu = true;
                DSPGame.EndGame();
            }
        }

        void OnDestroy()
        {
            // This make sure to disconnect if you force close the game.
            Disconnect();
        }

        void CleanupSession()
        {
            PlayerManager.RemoveAll();
        }

        void Update()
        {
            Client.Update();
        }

        private void OnJoinSessionConfirmed(JoinSessionConfirmed packet)
        {
            PlayerManager.SetLocalPlayer(packet.LocalPlayerId);
        }

        private void OnRemotePlayerJoined(RemotePlayerJoined packet)
        {
            PlayerManager.AddRemotePlayer(packet.PlayerId);
        }

        private void OnRemotePlayerDisconnect(PlayerDisconnected packet)
        {
            PlayerManager.RemovePlayer(packet.PlayerId);
        }

        private void OnPlayerMovement(Movement packet)
        {
            PlayerManager.GetPlayerModelById(packet.PlayerId)?.Movement.UpdatePosition(packet);
        }

        private void OnPlayerAnimationUpdate(PlayerAnimationUpdate packet)
        {
            PlayerManager.GetPlayerModelById(packet.PlayerId)?.Animator.UpdateState(packet);
        }

        private void OnVegeMined(VegeMined packet)
	    {
            PlanetData planet = null;

            if(GameMain.localPlanet?.id == packet.PlanetID)
            {
                planet = GameMain.localPlanet;
                if(planet == null)
                {
                    return;
                }
            }
            else
            {
                planet = GameMain.galaxy?.PlanetById(packet.PlanetID);
                if(planet == null)
                {
                    return;
                }
            }

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

                        if(planet.id == GameMain.localPlanet?.id)
                        {
                            VFEffectEmitter.Emit(vProto.MiningEffect, vData.pos, Maths.SphericalRotation(vData.pos, 0f));
                            VFAudio.Create(vProto.MiningAudio, null, vData.pos, true);
                        }
                    }
                    else
                    {
                        PlanetData.VeinGroup[] vGroups = planet.factory?.planet.veinGroups;
                        vGroups[vData.groupIndex].count -= 1;

                        if(planet.id == GameMain.localPlanet?.id)
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
