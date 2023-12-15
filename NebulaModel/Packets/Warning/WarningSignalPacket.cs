namespace NebulaModel.Packets.Warning;

public class WarningSignalPacket
{
    public int SignalCount { get; set; }
    public int[] Signals { get; set; }
    public int[] Counts { get; set; }
    public int Tick { get; set; }
}
