#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory;

[RegisterPacketProcessor]
internal class CreatePrebuildsRequestProcessor : PacketProcessor<CreatePrebuildsRequest>
{
    protected override void ProcessPacket(CreatePrebuildsRequest packet, NebulaConnection conn)
    {
        using (Multiplayer.Session.Factories.IsIncomingRequest.On())
        {
            // setting specifyPlanet here to avoid accessing a null object (see GPUInstancingManager activePlanet getter)
            var pData = GameMain.gpuiManager.specifyPlanet;

            GameMain.gpuiManager.specifyPlanet = GameMain.galaxy.PlanetById(packet.PlanetId);
            Multiplayer.Session.BuildTools.CreatePrebuildsRequest(packet);
            GameMain.gpuiManager.specifyPlanet = pData;
        }
    }
}
