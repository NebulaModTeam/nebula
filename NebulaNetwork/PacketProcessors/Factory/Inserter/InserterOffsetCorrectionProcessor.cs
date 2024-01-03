#region

using NebulaAPI.Networking;
using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Packets.Factory.Inserter;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Inserter;

[RegisterPacketProcessor]
internal class InserterOffsetCorrectionProcessor : BasePacketProcessor<InserterOffsetCorrectionPacket>
{
    public override void ProcessPacket(InserterOffsetCorrectionPacket packet, INebulaConnection conn)
    {
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factorySystem?.inserterPool;
        if (pool == null)
        {
            return;
        }
        Log.Warn(
            $"{packet.PlanetId} Fix inserter{packet.InserterId} pickOffset->{packet.PickOffset} insertOffset->{packet.InsertOffset}");
        pool[packet.InserterId].pickOffset = packet.PickOffset;
        pool[packet.InserterId].insertOffset = packet.InsertOffset;
    }
}
