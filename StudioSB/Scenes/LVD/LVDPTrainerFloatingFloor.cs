using StudioSB.Tools;
using System.ComponentModel;

namespace StudioSB.Scenes.LVD
{
    public class LVDPTrainerFloatingFloor : LVDBase
    {
        [ReadOnly(true), Category("Version")]
        public byte Version { get; internal set; } = 1;

        [Category("Position"), TypeConverter(typeof(ExpandableObjectConverter))]
        public LVDVector3 Position { get; set; } = new LVDVector3(0.0f, 0.0f, 0.0f);

        public override void Read(BinaryReaderExt reader)
        {
            Version = reader.ReadByte();

            base.Read(reader);

            Position.Read(reader);
        }

        public override void Write(BinaryWriterExt writer)
        {
            writer.Write(Version);

            base.Write(writer);

            Position.Write(writer);
        }
    }
}
