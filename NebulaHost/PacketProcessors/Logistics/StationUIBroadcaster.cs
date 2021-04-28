using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using NebulaWorld;
using NebulaWorld.Logistics;
using System.Collections.Generic;

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
            // if a user adds/removes a ship, drone or warper broadcast to everyone.
            if((packet.settingIndex == StationUI.UIsettings.MaxChargePower || packet.settingIndex == StationUI.UIsettings.setDroneCount || packet.settingIndex == StationUI.UIsettings.setShipCount || packet.settingIndex == StationUI.UIsettings.setWarperCount) && player != null && StationUIManager.UpdateCooldown == 0)
            {
                playerManager.SendPacketToAllPlayers(packet);
            }
            else if(StationUIManager.UpdateCooldown == 0 || !packet.isStorageUI)
            {
                List<NebulaConnection> subscribers = StationUIManager.GetSubscribers(packet.stationGId);
                for (int i = 0; i < subscribers.Count; i++)
                {
                    if(subscribers[i] != null)
                    {
                        if(subscribers[i] == conn)
                        {
                            packet.shouldMimick = true;
                        }
                        subscribers[i].SendPacket(packet);
                    }
                }
            }
            SimulatedWorld.OnStationUIChange(packet);
        }
    }
}
