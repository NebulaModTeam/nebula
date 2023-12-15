#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Planet;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Planet;

[RegisterPacketProcessor]
public class PlanetDetailResponseProcessor : PacketProcessor<PlanetDetailResponse>
{
    protected override void ProcessPacket(PlanetDetailResponse packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            return;
        }


        var planet = Multiplayer.Session.IsInLobby
            ? UIRoot.instance.galaxySelect.starmap._galaxyData.PlanetById(packet.PlanetDataID)
            : GameMain.galaxy.PlanetById(packet.PlanetDataID);

        if (packet.VeinCounts.Length > 0)
        {
            if (planet.veinGroups == null || planet.veinGroups.Length != packet.VeinCounts.Length)
            {
                planet.veinGroups = new VeinGroup[packet.VeinCounts.Length];
            }
            for (var i = 1; i < planet.veinGroups.Length; i++)
            {
                planet.veinGroups[i].type = (EVeinType)packet.VeinTypes[i];
                planet.veinGroups[i].count = packet.VeinCounts[i];
                planet.veinGroups[i].amount = packet.VeinAmounts[i];
            }
        }
        planet.landPercent = packet.LandPercent;
        planet.landPercentDirty = false;

        //planet.NotifyCalculated();
        planet.calculating = false;
        planet.calculated = true;
    }
}
