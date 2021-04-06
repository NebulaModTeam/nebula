using NebulaModel.Networking;

namespace NebulaModel.Packets.Factory
{
    public class AddEntityPreviewRequest
    {
        public int PlanetId { get; set; }
        public byte[] PrebuildDataRaw { get; set; }

        public AddEntityPreviewRequest() { }
        public AddEntityPreviewRequest(int planetId, PrebuildData prebuild) 
        {
            PlanetId = planetId;

            using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
            {
                prebuild.Export(writer.BinaryWriter);
                PrebuildDataRaw = writer.CloseAndGetBytes();
            }
        }

        public PrebuildData GetPrebuildData()
        {
            PrebuildData prebuild = new PrebuildData();
            using (BinaryUtils.Reader writer = new BinaryUtils.Reader(PrebuildDataRaw))
            {
                prebuild.Import(writer.BinaryReader);
            }
            return prebuild;
        }
    }
}
