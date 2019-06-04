using StudioSB.Tools;

namespace StudioSB.Scenes.LVD
{
    public class LVDGeneralShape : LVDEntry
    {
        public int ID { get; set; }
        public LVDShape Shape { get; set; }

        public override void Read(BinaryReaderExt r)
        {
            base.Read(r);

            r.Skip(1);
            ID = r.ReadInt32();

            Shape = new LVDShape();
            Shape.Read(r);
        }

        public override void Write(BinaryWriterExt writer)
        {
            base.Write(writer);

            writer.Write((byte)1);
            writer.Write(ID);

            Shape.Write(writer);
        }
    }
}
