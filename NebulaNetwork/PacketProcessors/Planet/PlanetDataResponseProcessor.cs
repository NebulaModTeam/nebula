#region

using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Planet;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Planet;

[RegisterPacketProcessor]
public class PlanetDataResponseProcessor : PacketProcessor<PlanetDataResponse>
{
    protected override void ProcessPacket(PlanetDataResponse packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            return;
        }


        var planet = Multiplayer.Session.IsInLobby
            ? UIRoot.instance.galaxySelect.starmap._galaxyData.PlanetById(packet.PlanetDataID)
            : GameMain.galaxy.PlanetById(packet.PlanetDataID);

        Log.Info($"Parsing {packet.PlanetDataByte.Length} bytes of data for planet {planet.name} (ID: {planet.id})");

        using (var reader = new BinaryUtils.Reader(packet.PlanetDataByte))
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

                var localPlanetId = Multiplayer.Session.LocalPlayer.Data.LocalPlanetId;
                if (planet.id != localPlanetId)
                {
                    return;
                }
                // Make local planet load first
                while (PlanetModelingManager.genPlanetReqList.Peek().id != localPlanetId)
                {
                    var tmp = PlanetModelingManager.genPlanetReqList.Dequeue();
                    PlanetModelingManager.genPlanetReqList.Enqueue(tmp);
                }
            }
        }
    }
}
