#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.PowerTower;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.PowerTower;

[RegisterPacketProcessor]
internal class PowerTowerChargerUpdateProcessor : PacketProcessor<PowerTowerChargerUpdate>
{
    protected override void ProcessPacket(PowerTowerChargerUpdate packet, NebulaConnection conn)
    {
        if (packet.PlanetId == -1)
        {
            // When a player connects, disconnects, or leaves planet, clear all records and restart
            Multiplayer.Session.PowerTowers.LocalChargerIds.Clear();
            Multiplayer.Session.PowerTowers.RemoteChargerHashIds.Clear();
            if (IsHost)
            {
                // Broadcast the leave planet event to other players 
                Multiplayer.Session.Network.SendPacket(packet);
            }
            return;
        }

        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory is not { powerSystem: not null })
        {
            return;
        }
        var hashId = ((long)packet.PlanetId << 32) | (long)packet.NodeId;
        if (packet.Charging)
        {
            if (Multiplayer.Session.PowerTowers.RemoteChargerHashIds.TryGetValue(hashId, out var playerCount))
            {
                Multiplayer.Session.PowerTowers.RemoteChargerHashIds[hashId] = playerCount + 1;
            }
            else
            {
                Multiplayer.Session.PowerTowers.RemoteChargerHashIds.Add(hashId, 1);
            }
            // Add remote charger [{packet.PlanetId}-{packet.NodeId}]: {Multiplayer.Session.PowerTowers.RemoteChargerHashIds[hashId]}
        }
        else
        {
            if (!Multiplayer.Session.PowerTowers.RemoteChargerHashIds.TryGetValue(hashId, out var playerCount))
            {
                return;
            }
            // Remove remote charger [{packet.PlanetId}-{packet.NodeId}]: {Multiplayer.Session.PowerTowers.RemoteChargerHashIds[hashId] - 1}
            Multiplayer.Session.PowerTowers.RemoteChargerHashIds[hashId] = playerCount - 1;
            if (playerCount <= 1)
            {
                Multiplayer.Session.PowerTowers.RemoteChargerHashIds.Remove(hashId);
            }
        }
    }
}
