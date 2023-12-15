namespace NebulaModel.Packets.GameStates;

public class FragmentInfo
{
    public FragmentInfo() { }

    public FragmentInfo(int size)
    {
        Size = size;
    }

    public int Size { get; set; }
}
