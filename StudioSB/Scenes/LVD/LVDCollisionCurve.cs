using StudioSB.Tools;

namespace StudioSB.Scenes.LVD
{
    public class LVDCollisionCurve : LVDEntry
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public float X { get; set; } = 1;

        public float Y { get; set; } = 1;

        public float Z { get; set; } = 1;

        public float W { get; set; } = 1;

        public int Unknown1 { get; set; } = 0;

        public int Unknown2 { get; set; } = 0;

        public override void Read(BinaryReaderExt r)
        {
            base.Read(r);
            
            ID = r.ReadInt32();

            r.Skip(1);
            Name = r.ReadString(0x40);
            
            X = r.ReadSingle();
            Y = r.ReadSingle();
            Z = r.ReadSingle();
            W = r.ReadSingle();

            Unknown1 = r.ReadInt32();
            Unknown2 = r.ReadInt32();
        }

        public override void Write(BinaryWriterExt writer)
        {
            base.Write(writer);

            writer.Write(ID);

            writer.Write((byte)1);
            writer.Write(Name.ToCharArray());
            writer.Write(new byte[0x40 - Name.Length]);

            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
            writer.Write(W);

            writer.Write(Unknown1);
            writer.Write(Unknown2);
        }
    }
}
