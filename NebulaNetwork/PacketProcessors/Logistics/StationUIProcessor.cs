using NebulaAPI;
using NebulaModel;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using System.Collections.Generic;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class StationUIProcessor : PacketProcessor<StationUI>
    {
        private IPlayerManager playerManager;
        public StationUIProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(StationUI packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                // if a user adds/removes a ship, drone or warper or changes max power input broadcast to everyone.
                if (Multiplayer.Session.StationsUI.UpdateCooldown == 0 &&
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
                            NebulaPlayer p = kvp.Value;
                            packet.ShouldMimic = p.Connection == conn;
                            p.SendPacket(packet);
                        }
                    }
                }
                else if (packet.SettingIndex == StationUI.EUISettings.AddOrRemoveItemFromStorageResponse)
                {
                    // if someone adds or removes items by hand broadcast to every player on that planet
                    NebulaPlayer player = playerManager.GetPlayer(conn);
                    if (player != null)
                    {
                        playerManager.SendPacketToPlanet(packet, player.Data.LocalPlanetId);
                    }
                }
                else if (Multiplayer.Session.StationsUI.UpdateCooldown == 0 || !packet.IsStorageUI)
                {
                    List<NebulaConnection> subscribers = Multiplayer.Session.StationsUI.GetSubscribers(packet.PlanetId, packet.StationId, packet.StationGId);

                    for (int i = 0; i < subscribers.Count; i++)
                    {
                        if (subscribers[i] != null)
                        {
                            /*
                             * as we block the normal method for the client he must run it once he receives this packet.
                             * but only the one issued the request should do it, we indicate this here
                             */
                            packet.ShouldMimic = subscribers[i] == conn;
                            subscribers[i].SendPacket(packet);
                        }
                    }
                }
                // always update values for host, but he does not need to rely on the mimic flag (infact its bad for him)
                packet.ShouldMimic = false;

                Multiplayer.Session.StationsUI.UpdateUI(packet);
            }

            if (IsClient)
            {
                Multiplayer.Session.StationsUI.UpdateUI(packet);
            }
        }
    }
}
