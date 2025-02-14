#region

using System;
using NebulaAPI;
using NebulaAPI.Packets;
using NebulaModel.DataStructures.Chat;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Chat;
using NebulaModel.Packets.Factory;
using NebulaWorld;
using NebulaWorld.Factory;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory;

[RegisterPacketProcessor]
public class UpgradeEntityRequestProcessor : PacketProcessor<UpgradeEntityRequest>
{
    protected override void ProcessPacket(UpgradeEntityRequest packet, NebulaConnection conn)
    {
        using (Multiplayer.Session.Factories.IsIncomingRequest.On())
        {
            var planet = GameMain.galaxy.PlanetById(packet.PlanetId);

            // We only execute the code if the client has loaded the factory at least once.
            // Else they will get it once they go to the planet for the first time. 
            if (planet?.factory == null)
            {
                return;
            }

            var localProtoId = FactoryManager.GetObjectProtoId(planet?.factory, packet.ObjId);
            if (localProtoId != packet.OriginProtoId)
            {
                var log = $"UpgradeEntityRequest rejected on planet {packet.PlanetId} for object {packet.ObjId}: {localProtoId} != {packet.OriginProtoId}";
                if (IsHost)
                {
                    Log.Warn(log);
                    var response = "Server rejected upgrade request due to protoId desync".Translate();
                    conn.SendPacket(new NewChatMessagePacket(ChatMessageType.SystemWarnMessage, response, DateTime.Now, ""));
                }
                else
                {
                    Log.WarnInform(log);
                }
                return;
            }

            Multiplayer.Session.Factories.TargetPlanet = packet.PlanetId;
            Multiplayer.Session.Factories.PacketAuthor = packet.AuthorId;

            Multiplayer.Session.Factories.AddPlanetTimer(packet.PlanetId);
            var itemProto = LDB.items.Select(packet.UpgradeProtoId);

            // setting specifyPlanet here to avoid accessing a null object (see GPUInstancingManager activePlanet getter)
            var pData = GameMain.gpuiManager.specifyPlanet;

            GameMain.gpuiManager.specifyPlanet = planet;
            planet.factory.UpgradeFinally(GameMain.mainPlayer, packet.ObjId, itemProto);
            GameMain.gpuiManager.specifyPlanet = pData;

            Multiplayer.Session.Factories.TargetPlanet = NebulaModAPI.PLANET_NONE;
            Multiplayer.Session.Factories.PacketAuthor = NebulaModAPI.AUTHOR_NONE;
        }
    }
}
