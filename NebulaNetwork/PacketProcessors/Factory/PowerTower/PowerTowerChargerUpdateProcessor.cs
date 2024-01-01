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
            // When a player disconnect, clear all records and restart
            Multiplayer.Session.PowerTowers.LocalChargerIds.Clear();
            Multiplayer.Session.PowerTowers.RemoteChargerHashIds.Clear();
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
            Multiplayer.Session.PowerTowers.RemoteChargerHashIds.Add(hashId);
        }
        else
        {
            Multiplayer.Session.PowerTowers.RemoteChargerHashIds.Remove(hashId);
        }
    }
}
