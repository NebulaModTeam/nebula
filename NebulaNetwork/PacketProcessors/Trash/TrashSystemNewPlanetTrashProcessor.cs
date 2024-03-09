#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Trash;
using NebulaWorld;
using NebulaWorld.Trash;
using UnityEngine;

#endregion

namespace NebulaNetwork.PacketProcessors.Trash;

[RegisterPacketProcessor]
internal class TrashSystemNewPlanetTrashProcessor : PacketProcessor<TrashSystemNewPlanetTrashPacket>
{
    protected override void ProcessPacket(TrashSystemNewPlanetTrashPacket packet, NebulaConnection conn)
    {
        var planet = GameMain.galaxy.PlanetById(packet.PlanetId);
        var factory = planet?.factory;
        if (factory == null) return;

        if (IsClient)
        {
            TrashManager.SetNextTrashId(packet.TrashId);
        }

        //Modify from AddTrashOnPlanet
        var trashSystem = GameMain.data.trashSystem;
        var vectorLF = new VectorLF3(packet.Pos.x, packet.Pos.y, packet.Pos.z);
        vectorLF += vectorLF.normalized;
        var normalized = Maths.QRotateLF(planet.runtimeRotation, vectorLF).normalized;
        var vectorLF2 = Maths.QRotateLF(planet.runtimeRotation, vectorLF) + planet.uPosition;
        var astroId = planet.star.astroId;
        var starGravity = trashSystem.GetStarGravity(planet.star.id);
        var trashData = default(TrashData);
        trashData.landPlanetId = 0;
        trashData.nearPlanetId = 0;
        trashData.nearStarId = astroId;
        trashData.nearStarGravity = starGravity;
        trashData.life = packet.Life;
        trashData.lPos = Vector3.zero;
        trashData.lRot = Quaternion.identity;
        trashData.uPos = vectorLF2 + RandomTable.SphericNormal(ref trashSystem.randSeed, 0.2);
        trashData.uRot = Quaternion.LookRotation(RandomTable.SphericNormal(ref trashSystem.randSeed, 1.0).normalized);
        trashData.uVel = planet.GetUniversalVelocityAtLocalPoint(GameMain.gameTime, vectorLF) + normalized * 10.0 + RandomTable.SphericNormal(ref trashSystem.randSeed, 3.0);
        trashData.uAgl = RandomTable.SphericNormal(ref trashSystem.randSeed, 0.03);

        if (IsClient)
        {
            trashData.warningId = -1; // Wait until WarningDataPacket to assign warningId
            Multiplayer.Session.Trashes.ClientTrashCount++;
        }
        var trashObject = new TrashObject(packet.ItemId, packet.Count, packet.Inc, Maths.QInvRotateLF(trashSystem.gameData.relativeRot, trashData.uPos - trashSystem.gameData.relativePos), Quaternion.Inverse(trashSystem.gameData.relativeRot) * trashData.uRot);

        using (Multiplayer.Session.Trashes.IsIncomingRequest.On())
        {
            var trashId = trashSystem.container.NewTrash(trashObject, trashData);
            if (IsHost)
            {
                packet.TrashId = trashId;
                Server.SendPacketToPlanet(packet, packet.PlanetId);
                return;
            }
            if (trashId != packet.TrashId)
            {
                NebulaModel.Logger.Log.Warn($"TrashSystemNewPlanetTrashPacket mismatch: {packet.TrashId} => {trashId}");
            }
        }
    }
}
