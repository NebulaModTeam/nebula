using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Planet;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    public class PlanetDataResponseProcessor : PacketProcessor<PlanetDataResponse>
    {
        public override void ProcessPacket(PlanetDataResponse packet, NebulaConnection conn)
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

            Log.Info($"Parsing {packet.PlanetDataByte.Length} bytes of data for planet {planet.name} (ID: {planet.id})");

            using (BinaryUtils.Reader reader = new BinaryUtils.Reader(packet.PlanetDataByte))
            {
                planet.ImportRuntime(reader.BinaryReader);
            }

            if (Multiplayer.Session.IsInLobby)
            {
                // Pretend planet is loaded to make planetDetail show resources
                planet.loaded = true;
                UIRoot.instance.uiGame.planetDetail.RefreshDynamicProperties();
            }
            else
            {
                lock (PlanetModelingManager.genPlanetReqList)
                {
                    PlanetModelingManager.genPlanetReqList.Enqueue(planet);
                }
            }
        }
    }
}
