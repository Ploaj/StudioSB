using StudioSB.Tools;
using System.ComponentModel;

namespace StudioSB.Scenes.LVD
{
    public class LVDRegion : LVDBase
    {
        [ReadOnly(true), Category("Version")]
        public byte Version { get; internal set; } = 2;

        [Category("Rectangle"), TypeConverter(typeof(ExpandableObjectConverter))]
        public LVDRect Rectangle { get; set; } = new LVDRect();

        public override void Read(BinaryReaderExt reader)
        {
            Version = reader.ReadByte();

            if (Version < 2)
            {
                MetaInfo.Read(reader);
            }
            else
            {
                base.Read(reader);
            }

            Rectangle.Read(reader);
        }

        public override void Write(BinaryWriterExt writer)
        {
            writer.Write(Version);

            if (Version < 2)
            {
                MetaInfo.Write(writer);
            }
            else
            {
                base.Write(writer);
            }

            Rectangle.Write(writer);
        }
    }
}
