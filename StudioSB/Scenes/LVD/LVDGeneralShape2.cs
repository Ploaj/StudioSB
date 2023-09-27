using StudioSB.Tools;
using System.ComponentModel;

namespace StudioSB.Scenes.LVD
{
    public class LVDGeneralShape2 : LVDBase
    {
        [ReadOnly(true), Category("Version")]
        public byte Version { get; internal set; } = 1;

        [Category("ID")]
        public int Tag { get; set; }

        [Category("Shape"), TypeConverter(typeof(ExpandableObjectConverter))]
        public LVDShape2 Shape { get; set; }

        public override void Read(BinaryReaderExt reader)
        {
            Version = reader.ReadByte();

            base.Read(reader);

            reader.Skip(1);
            Tag = reader.ReadInt32();

            Shape = new LVDShape2();
            Shape.Read(reader);
        }

        public override void Write(BinaryWriterExt writer)
        {
            writer.Write(Version);

            base.Write(writer);

            writer.Write((byte)1);
            writer.Write(Tag);

            Shape.Write(writer);
        }
    }
}
