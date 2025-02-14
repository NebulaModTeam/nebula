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
public class DestructEntityRequestProcessor : PacketProcessor<DestructEntityRequest>
{
    protected override void ProcessPacket(DestructEntityRequest packet, NebulaConnection conn)
    {
        using (Multiplayer.Session.Factories.IsIncomingRequest.On())
        {
            var planet = GameMain.galaxy.PlanetById(packet.PlanetId);
            var pab = GameMain.mainPlayer.controller != null ? GameMain.mainPlayer.controller.actionBuild : null;

            // We only execute the code if the client has loaded the factory at least once.
            // Else they will get it once they go to the planet for the first time. 
            if (planet?.factory == null || pab == null)
            {
                return;
            }

            var localProtoId = FactoryManager.GetObjectProtoId(planet?.factory, packet.ObjId);
            if (localProtoId != packet.ProtoId)
            {
                // Either the object is already destroyed on server (0), id out of bound (-1) or object mismatch.
                // Omit the first case. (Somehow area dismantle often trigger the first case)
                if (localProtoId != 0)
                {
                    var log = $"DestructEntityRequest rejected on planet {packet.PlanetId} for object {packet.ObjId}: {localProtoId} != {packet.ProtoId}";
                    if (IsHost)
                    {
                        Log.Warn(log);
                        var response = "Server rejected destruct request due to protoId desync".Translate();
                        conn.SendPacket(new NewChatMessagePacket(ChatMessageType.SystemWarnMessage, response, DateTime.Now, ""));
                    }
                    else
                    {
                        Log.WarnInform(log);
                    }
                }
                return;
            }

            Multiplayer.Session.Factories.TargetPlanet = packet.PlanetId;
            Multiplayer.Session.Factories.PacketAuthor = packet.AuthorId;
            var tmpFactory = pab.factory;
            pab.factory = planet.factory;
            pab.noneTool.factory = planet.factory;

            Multiplayer.Session.Factories.AddPlanetTimer(packet.PlanetId);

            // setting specifyPlanet here to avoid accessing a null object (see GPUInstancingManager activePlanet getter)
            var pData = GameMain.gpuiManager.specifyPlanet;

            GameMain.gpuiManager.specifyPlanet = GameMain.galaxy.PlanetById(packet.PlanetId);
            pab.DoDismantleObject(packet.ObjId);
            GameMain.gpuiManager.specifyPlanet = pData;

            pab.factory = tmpFactory;
            pab.noneTool.factory = tmpFactory;
            Multiplayer.Session.Factories.TargetPlanet = NebulaModAPI.PLANET_NONE;
            Multiplayer.Session.Factories.PacketAuthor = NebulaModAPI.AUTHOR_NONE;
        }
    }
}
