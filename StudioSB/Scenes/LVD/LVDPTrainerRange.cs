using StudioSB.Tools;
using System.Collections.Generic;
using System.ComponentModel;

namespace StudioSB.Scenes.LVD
{
    public class LVDPTrainerRange : LVDBase
    {
        [ReadOnly(true), Category("Version")]
        public byte Version { get; internal set; } = 4;

        [Category("Range"), TypeConverter(typeof(ExpandableObjectConverter))]
        public LVDVector3 BoundaryMin { get; set; } = new LVDVector3(0.0f, 0.0f, 0.0f);

        [Category("Range"), TypeConverter(typeof(ExpandableObjectConverter))]
        public LVDVector3 BoundaryMax { get; set; } = new LVDVector3(0.0f, 0.0f, 0.0f);

        [Category("Range")]
        public List<LVDVector3> Trainers { get; set; } = new List<LVDVector3>();

        [Category("Parent")]
        public string ParentModelName { get; set; }

        [Category("Parent")]
        public string ParentJointName { get; set; }

        public override void Read(BinaryReaderExt reader)
        {
            Version = reader.ReadByte();

            base.Read(reader);

            BoundaryMin.Read(reader);

            BoundaryMax.Read(reader);

            reader.Skip(1);
            uint trainerCount = reader.ReadUInt32();
            for (uint i = 0; i < trainerCount; i++)
            {
                LVDVector3 trainer = new LVDVector3(0.0f, 0.0f, 0.0f);

                trainer.Read(reader);
                Trainers.Add(trainer);
            }

            if (Version > 1)
            {
                reader.Skip(1);
                ParentModelName = reader.ReadString();
                reader.Skip(0x40 - (uint)ParentModelName.Length - 1);

                reader.Skip(1);
                ParentJointName = reader.ReadString();
                reader.Skip(0x40 - (uint)ParentJointName.Length - 1);
            }
        }

        public override void Write(BinaryWriterExt writer)
        {
            writer.Write(Version);

            base.Write(writer);

            BoundaryMin.Write(writer);

            BoundaryMax.Write(writer);

            writer.Write((byte)1);
            writer.Write(Trainers.Count);
            foreach (var v in Trainers)
            {
                v.Write(writer);
            }

            if (Version > 1)
            {
                writer.Write((byte)1);
                writer.Write(ParentModelName);
                writer.Write(new byte[0x40 - ParentModelName.Length - 1]);

                writer.Write((byte)1);
                writer.Write(ParentJointName);
                writer.Write(new byte[0x40 - ParentJointName.Length - 1]);
            }
        }
    }
}
