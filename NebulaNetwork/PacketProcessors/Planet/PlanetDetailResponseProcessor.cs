using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Planet;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    public class PlanetDetailResponseProcessor : PacketProcessor<PlanetDetailResponse>
    {
        public override void ProcessPacket(PlanetDetailResponse packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                return;
            }


            PlanetData planet = null;

            if (Multiplayer.Session.IsInLobby)
            {
                planet = UIRoot.instance.galaxySelect.starmap._galaxyData.PlanetById(packet.PlanetDataID);
            }
            else
            {
                planet = GameMain.galaxy.PlanetById(packet.PlanetDataID);
            }

            if (planet.veinGroups == null || planet.veinGroups.Length != packet.VeinCounts.Length)
            {
                planet.veinGroups = new VeinGroup[packet.VeinCounts.Length];
            }
            for (int i = 1; i < planet.veinGroups.Length; i++)
            {
                planet.veinGroups[i].type = (EVeinType)packet.VeinTypes[i];
                planet.veinGroups[i].count = packet.VeinCounts[i];
                planet.veinGroups[i].amount = packet.VeinAmounts[i];
            }
            planet.landPercent = packet.LandPercent;
            planet.landPercentDirty = false;

            //planet.NotifyCalculated();
            planet.calculating = false;
            planet.calculated = true;
        }
    }
}
