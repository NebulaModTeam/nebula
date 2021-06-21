using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NebulaModel.Packets.Universe
{
    // Packet for name input for Planets and Stars
    public class NameInputPacket
    {
        public string Name { get; set; }
        public int StellarId { get; set; }
        public int AuthorId { get; set; }

        public NameInputPacket() { }
        public NameInputPacket(string name, int stellarId, int authorId)
        {
            this.Name = name;
            this.StellarId = stellarId;
            this.AuthorId = authorId;
        }
    }
}
