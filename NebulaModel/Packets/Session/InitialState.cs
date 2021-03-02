namespace NebulaModel.Packets.Session
{
    public class InitialState
    {
        public string URI { get; set; }

        public InitialState() { }

        public InitialState(string uri)
        {
            this.URI = uri;
        }
    }
}
