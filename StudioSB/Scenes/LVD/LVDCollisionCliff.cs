using StudioSB.Tools;
using System.ComponentModel;

namespace StudioSB.Scenes.LVD
{
    public class LVDCollisionCliff : LVDBase
    {
        [ReadOnly(true), Category("Version")]
        public byte Version { get; internal set; } = 3;

        [Category("Values"), TypeConverter(typeof(ExpandableObjectConverter))]
        public LVDVector2 Position { get; set; } = new LVDVector2(0.0f, 0.0f);

        [Category("Values")]
        public float Lr { get; set; }

        [Category("Values")]
        public int LineIndex { get; set; }

        public override void Read(BinaryReaderExt reader)
        {
            Version = reader.ReadByte();

            if (Version > 1)
            {
                base.Read(reader);
            }

            Position.Read(reader);

            Lr = reader.ReadSingle();

            if (Version < 3)
            {
                return;
            }

            LineIndex = reader.ReadInt32();
        }

        public override void Write(BinaryWriterExt writer)
        {
            writer.Write(Version);

            if (Version > 1)
            {
                base.Write(writer);
            }

            Position.Write(writer);

            writer.Write(Lr);

            if (Version < 3)
            {
                return;
            }

            writer.Write(LineIndex);
        }
    }
}
