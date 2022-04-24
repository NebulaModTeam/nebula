﻿using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    internal class DysonSphereRemoveShellProcessor : PacketProcessor<DysonSphereRemoveShellPacket>
    {
        public override void ProcessPacket(DysonSphereRemoveShellPacket packet, NebulaConnection conn)
        {
            DysonSphereLayer layer = GameMain.data.dysonSpheres[packet.StarIndex]?.GetLayer(packet.LayerId);
            if (layer == null)
            {
                return;
            }
            using (Multiplayer.Session.DysonSpheres.IsIncomingRequest.On())
            {
                if (packet.ShellId < 1 || packet.ShellId >= layer.shellCursor)
                {
                    Log.Warn($"Cannnot remove shell[{packet.ShellId}] on layer[{layer.id}], starIndex[{packet.StarIndex}]");
                    Multiplayer.Session.DysonSpheres.HandleDesync(packet.StarIndex, conn);
                    return;
                }
                //No need to remove if the shell is already null
                if (layer.shellPool[packet.ShellId] != null)
                {
                    layer.RemoveDysonShell(packet.ShellId);
                    NebulaWorld.Universe.DysonSphereManager.ClearSelection(packet.StarIndex, layer.id);
                }
            }
            if (IsHost)
            {
                Multiplayer.Session.DysonSpheres.SendPacketToDysonSphereExcept(packet, packet.StarIndex, conn);
            }
        }
    }
}
