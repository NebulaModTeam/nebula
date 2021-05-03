using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using NebulaWorld;
using NebulaWorld.Logistics;
using System.Collections.Generic;

/*
 * This packet covers updates to the UIStationWindow and UIStationStorage
 * some gets sent to everyone (see below), some only to the ones having the same UI window opened
 */
namespace NebulaHost.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class StationUIBroadcaster: IPacketProcessor<StationUI>
    {
        private PlayerManager playerManager;
        public StationUIBroadcaster()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }
        public void ProcessPacket(StationUI packet, NebulaConnection conn)
        {
            // if a user adds/removes a ship, drone or warper or changes max power input broadcast to everyone.
            if (StationUIManager.UpdateCooldown == 0 &&
                (packet.SettingIndex == StationUI.EUISettings.MaxChargePower
                 || packet.SettingIndex == StationUI.EUISettings.SetDroneCount
                 || packet.SettingIndex == StationUI.EUISettings.SetShipCount
                 || packet.SettingIndex == StationUI.EUISettings.SetWarperCount)
                )
            {
                // this is the SendPacketToAllPlayers() logic but we need to set the mimic flag here.
                using (playerManager.GetConnectedPlayers(out var connectedPlayers))
                {
                    foreach (var kvp in connectedPlayers)
                    {
                        Player p = kvp.Value;
                        if (p.Connection == conn)
                        {
                            packet.ShouldMimic = true;
                            p.SendPacket(packet);
                            packet.ShouldMimic = false;
                        }
                        else
                        {
                            p.SendPacket(packet);
                        }
                    }
                }
            }
            else if (packet.SettingIndex == StationUI.EUISettings.AddOrRemoveItemFromStorageResponse)
            {
                // if someone adds or removes items by hand broadcast to every player on that planet
                Player player = playerManager.GetPlayer(conn);
                if (player != null)
                {
                    playerManager.SendPacketToPlanet(packet, player.Data.LocalPlanetId);
                }
            }
            else if(StationUIManager.UpdateCooldown == 0 || !packet.IsStorageUI)
            {
                List<NebulaConnection> subscribers = StationUIManager.GetSubscribers(packet.PlanetId, packet.StationId, packet.StationGId);
                
                for (int i = 0; i < subscribers.Count; i++)
                {
                    if(subscribers[i] != null)
                    {
                        if(subscribers[i] == conn)
                        {
                            /*
                             * as we block the normal method for the client he must run it once he receives this packet.
                             * but only the one issued the request should do it, we indicate this here
                             */
                            packet.ShouldMimic = true;
                        }
                        subscribers[i].SendPacket(packet);
                    }
                }
            }
            // always update values for host, but he does not need to rely on the mimic flag (infact its bad for him)
            packet.ShouldMimic = false;
            SimulatedWorld.OnStationUIChange(packet);
        }
    }
}
