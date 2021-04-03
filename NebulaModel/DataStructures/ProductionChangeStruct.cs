using NebulaModel.Attributes;
using System.IO;

namespace NebulaModel.DataStructures
{
    [RegisterNestedType]
    public struct ProductionChangeStruct //12 bytes total
    {
        public bool IsProduction; //1-byte
        public ushort ProductId; //2-byte
        public int Amount;  //4-byte

        public ProductionChangeStruct(bool isProduction, ushort productId, int amount)
        {
            this.IsProduction = isProduction;
            this.ProductId = productId;
            this.Amount = amount;
        }

        public ProductionChangeStruct(BinaryReader r)
        {
            IsProduction = r.ReadBoolean();
            ProductId = r.ReadUInt16();
            Amount = r.ReadInt32();
        }

        public void Export(BinaryWriter w)
        {
            w.Write(IsProduction);
            w.Write(ProductId);
            w.Write(Amount);
        }
    }
}
