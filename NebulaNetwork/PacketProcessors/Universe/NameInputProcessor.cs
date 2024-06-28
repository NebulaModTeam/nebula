#region

using NebulaAPI;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Universe;

/*
 * Receives change event for name of planet or star and applies the change
 */
[RegisterPacketProcessor]
internal class NameInputProcessor : PacketProcessor<NameInputPacket>
{
    protected override void ProcessPacket(NameInputPacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            var player = Players.Get(conn);
            if (player != null)
            {
                Server.SendPacketExclude(packet, conn);
            }
        }

        using (Multiplayer.Session.Factories.IsIncomingRequest.On())
        {
            // If in lobby, apply change to UI galaxy
            var galaxyData = Multiplayer.Session.IsInLobby ? UIRoot.instance.galaxySelect.starmap.galaxyData : GameMain.galaxy;
            if (galaxyData == null)
            {
                return;
            }

            for (var i = 0; i < packet.Names.Length; i++)
            {
                if (packet.StarIds[i] != NebulaModAPI.STAR_NONE)
                {
                    var star = galaxyData.StarById(packet.StarIds[i]);
                    star.overrideName = packet.Names[i];
                    star.NotifyOnDisplayNameChange();
                }
                else
                {
                    var planet = galaxyData.PlanetById(packet.PlanetIds[i]);
                    planet.overrideName = packet.Names[i];
                    planet.NotifyOnDisplayNameChange();
                }
            }
            galaxyData.NotifyAstroNameChange();
            if (Multiplayer.Session.IsInLobby)
            {
                // Refresh star name in lobby
                UIRoot.instance.galaxySelect.starmap.OnGalaxyDataReset();
            }
        }
    }
}
