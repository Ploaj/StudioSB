using StudioSB.Tools;
using System.ComponentModel;

namespace StudioSB.Scenes.LVD
{
    public class LVDPoint : LVDBase
    {
        [ReadOnly(true), Category("Version")]
        public byte Version { get; internal set; } = 2;

        [Category("Position"), TypeConverter(typeof(ExpandableObjectConverter))]
        public LVDVector2 Position { get; set; } = new LVDVector2(0.0f, 0.0f);

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

            Position.Read(reader);
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

            Position.Write(writer);
        }
    }
}
