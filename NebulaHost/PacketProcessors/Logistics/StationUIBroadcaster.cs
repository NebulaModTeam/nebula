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
            Player player = playerManager.GetPlayer(conn);
            // if a user adds/removes a ship, drone or warper or changes max power input broadcast to everyone.
            if (
                (
                    packet.SettingIndex == StationUI.EUISettings.MaxChargePower
                    || packet.SettingIndex == StationUI.EUISettings.SetDroneCount
                    || packet.SettingIndex == StationUI.EUISettings.SetShipCount
                    || packet.SettingIndex == StationUI.EUISettings.SetWarperCount
                    || packet.SettingIndex == StationUI.EUISettings.AddOrRemoveItemFromStorageRequest
                    || packet.SettingIndex == StationUI.EUISettings.AddOrRemoveItemFromStorageResponse
                )
                && player != null && StationUIManager.UpdateCooldown == 0
            )
            {
                playerManager.SendPacketToAllPlayers(packet);
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
            // always update values for host
            SimulatedWorld.OnStationUIChange(packet);
        }
    }
}
