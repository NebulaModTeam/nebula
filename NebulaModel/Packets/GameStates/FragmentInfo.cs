namespace NebulaModel.Packets.GameStates
{
    public class FragmentInfo
    {
        public int Size { get; set; }

        public FragmentInfo() { }
        public FragmentInfo(int size)
        {
            Size = size;
        }
    }
}
