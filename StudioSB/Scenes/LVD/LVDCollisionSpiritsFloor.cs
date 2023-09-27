using StudioSB.Tools;
using System.ComponentModel;

namespace StudioSB.Scenes.LVD
{
    public class LVDCollisionSpiritsFloor : LVDBase
    {
        [ReadOnly(true), Category("Version")]
        public byte Version { get; internal set; } = 2;

        [Category("Line")]
        public uint LineIndex { get; set; }

        [Category("Line")]
        public string LineGroup { get; set; }

        [Category("Values")]
        public float Unknown1 { get; set; } = 1.0f;

        [Category("Values")]
        public float Unknown2 { get; set; } = 1.0f;

        [Category("Values")]
        public float Unknown3 { get; set; } = 1.0f;

        [Category("Values")]
        public float Unknown4 { get; set; } = 1.0f;

        [Category("Values")]
        public float Unknown5 { get; set; } = 0.0f;

        [Category("Values")]
        public float Unknown6 { get; set; } = 0.0f;

        public override void Read(BinaryReaderExt reader)
        {
            Version = reader.ReadByte();

            base.Read(reader);

            LineIndex = reader.ReadUInt32();

            reader.Skip(1);
            LineGroup = reader.ReadString();
            reader.Skip(0x40 - (uint)LineGroup.Length - 1);

            if (Version < 2)
            {
                return;
            }

            Unknown1 = reader.ReadSingle();
            Unknown2 = reader.ReadSingle();
            Unknown3 = reader.ReadSingle();
            Unknown4 = reader.ReadSingle();
            Unknown5 = reader.ReadSingle();
            Unknown6 = reader.ReadSingle();
        }

        public override void Write(BinaryWriterExt writer)
        {
            writer.Write(Version);

            base.Write(writer);

            writer.Write(LineIndex);

            writer.Write((byte)1);
            writer.Write(LineGroup);
            writer.Write(new byte[0x40 - LineGroup.Length - 1]);

            if (Version < 2)
            {
                return;
            }

            writer.Write(Unknown1);
            writer.Write(Unknown2);
            writer.Write(Unknown3);
            writer.Write(Unknown4);
            writer.Write(Unknown5);
            writer.Write(Unknown6);
        }
    }
}
