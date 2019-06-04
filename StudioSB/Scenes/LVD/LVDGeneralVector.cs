using StudioSB.Tools;

namespace StudioSB.Scenes.LVD
{
    public class LVDGeneralVector : LVDEntry
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public override void Read(BinaryReaderExt r)
        {
            base.Read(r);

            r.ReadByte();

            X = r.ReadSingle();
            Y = r.ReadSingle();
            Z = r.ReadSingle();
        }

        public override void Write(BinaryWriterExt writer)
        {
            base.Write(writer);

            writer.Write((byte)1);

            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
        }
    }
}
