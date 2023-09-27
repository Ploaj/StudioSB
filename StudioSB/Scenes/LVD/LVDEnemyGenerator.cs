using StudioSB.Tools;
using System.Collections.Generic;
using System.ComponentModel;

namespace StudioSB.Scenes.LVD
{
    public class LVDEnemyGenerator : LVDBase
    {
        [ReadOnly(true), Category("Version")]
        public byte Version { get; internal set; } = 3;

        public List<LVDShape2> Unknown1 { get; set; } = new List<LVDShape2>();

        public List<LVDShape2> Unknown2 { get; set; } = new List<LVDShape2>();

        public List<LVDShape2> Unknown3 { get; set; } = new List<LVDShape2>();

        [Category("ID")]
        public int Tag { get; set; }

        public List<int> Unknown4 { get; set; } = new List<int>();

        public List<int> Unknown5 { get; set; } = new List<int>();

        public List<int> Unknown6 { get; set; } = new List<int>();

        public override void Read(BinaryReaderExt reader)
        {
            Version = reader.ReadByte();

            base.Read(reader);

            reader.Skip(1);
            reader.Skip(1);
            var unknown1Count = reader.ReadUInt32();
            for (uint i = 0; i < unknown1Count; i++)
            {
                reader.Skip(1);
                var shape = new LVDShape2();
                shape.Read(reader);
                Unknown1.Add(shape);
            }

            reader.Skip(1);
            reader.Skip(1);
            var unknown2Count = reader.ReadUInt32();
            for (uint i = 0; i < unknown2Count; i++)
            {
                reader.Skip(1);
                var shape = new LVDShape2();
                shape.Read(reader);
                Unknown2.Add(shape);
            }

            reader.Skip(1);
            reader.Skip(1);
            var unknown3Count = reader.ReadUInt32();
            for (uint i = 0; i < unknown3Count; i++)
            {
                reader.Skip(1);
                var shape = new LVDShape2();
                shape.Read(reader);
                Unknown3.Add(shape);
            }

            reader.Skip(1);
            Tag = reader.ReadInt32();

            if (Version < 2)
            {
                return;
            }

            reader.Skip(1);
            var unknown4Count = reader.ReadUInt32();
            for (int i = 0; i < unknown4Count; i++)
            {
                reader.Skip(1);
                Unknown4.Add(reader.ReadInt32());
            }

            reader.Skip(1);
            var unknown5Count = reader.ReadUInt32();
            for (int i = 0; i < unknown5Count; i++)
            {
                reader.Skip(1);
                Unknown5.Add(reader.ReadInt32());
            }

            if (Version < 3)
            {
                return;
            }

            reader.Skip(1);
            var unknown6Count = reader.ReadUInt32();
            for (int i = 0; i < unknown6Count; i++)
            {
                reader.Skip(1);
                Unknown6.Add(reader.ReadInt32());
            }
        }

        public override void Write(BinaryWriterExt writer)
        {
            writer.Write(Version);

            base.Write(writer);

            writer.Write((byte)1);
            writer.Write((byte)1);
            writer.Write(Unknown1.Count);
            foreach (var v in Unknown1)
            {
                writer.Write((byte)1);
                v.Write(writer);
            }

            writer.Write((byte)1);
            writer.Write((byte)1);
            writer.Write(Unknown2.Count);
            foreach (var v in Unknown2)
            {
                writer.Write((byte)1);
                v.Write(writer);
            }

            writer.Write((byte)1);
            writer.Write((byte)1);
            writer.Write(Unknown3.Count);
            foreach (var v in Unknown3)
            {
                writer.Write((byte)1);
                v.Write(writer);
            }

            writer.Write((byte)1);
            writer.Write(Tag);

            if (Version < 2)
            {
                return;
            }

            writer.Write((byte)1);
            writer.Write(Unknown4.Count);
            foreach (var v in Unknown4)
            {
                writer.Write((byte)1);
                writer.Write(v);
            }

            writer.Write((byte)1);
            writer.Write(Unknown5.Count);
            foreach (var v in Unknown5)
            {
                writer.Write((byte)1);
                writer.Write(v);
            }

            if (Version < 3)
            {
                return;
            }

            writer.Write((byte)1);
            writer.Write(Unknown6.Count);
            foreach (var v in Unknown6)
            {
                writer.Write((byte)1);
                writer.Write(v);
            }
        }
    }
}
