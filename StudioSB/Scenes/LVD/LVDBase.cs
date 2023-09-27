using StudioSB.Tools;
using System.ComponentModel;

namespace StudioSB.Scenes.LVD
{
    public class LVDBase
    {
        [ReadOnly(true), Category("Version")]
        public byte BaseVersion { get; internal set; } = 4;

        [Category("Base"), TypeConverter(typeof(ExpandableObjectConverter))]
        public LVDMetaInfo MetaInfo { get; set; } = new LVDMetaInfo();

        [Category("Base")]
        public string DynamicName { get; set; }

        [Category("Base"), TypeConverter(typeof(ExpandableObjectConverter))]
        public LVDVector3 DynamicOffset { get; set; } = new LVDVector3(0.0f, 0.0f, 0.0f);

        [Category("Base")]
        public bool IsDynamic { get; set; }

        [Category("Base")]
        public uint InstanceID { get; set; }

        [Category("Base"), TypeConverter(typeof(ExpandableObjectConverter))]
        public LVDVector3 InstanceOffset { get; set; } = new LVDVector3(0.0f, 0.0f, 0.0f);

        [Category("Base")]
        public int JointIndex { get; set; }

        [Category("Base")]
        public string JointName { get; set; }

        public virtual void Read(BinaryReaderExt reader)
        {
            BaseVersion = reader.ReadByte();

            MetaInfo.Read(reader);

            reader.Skip(1);
            DynamicName = reader.ReadString();
            reader.Skip(0x40 - (uint)DynamicName.Length - 1);

            if (BaseVersion < 2)
            {
                return;
            }

            DynamicOffset.Read(reader);

            if (BaseVersion < 3)
            {
                return;
            }

            IsDynamic = reader.ReadBoolean();

            reader.Skip(1);
            InstanceID = reader.ReadUInt32();

            InstanceOffset.Read(reader);

            if (BaseVersion < 4)
            {
                return;
            }
            
            JointIndex = reader.ReadInt32();

            reader.Skip(1);
            JointName = reader.ReadString();
            reader.Skip(0x40 - (uint)JointName.Length - 1);
        }

        public virtual void Write(BinaryWriterExt writer)
        {
            writer.Write(BaseVersion);

            MetaInfo.Write(writer);

            writer.Write((byte)1);
            writer.Write(DynamicName);
            writer.Write(new byte[0x40 - DynamicName.Length - 1]);

            if (BaseVersion < 2)
            {
                return;
            }

            DynamicOffset.Write(writer);

            if (BaseVersion < 3)
            {
                return;
            }

            writer.Write(IsDynamic);

            writer.Write((byte)1);
            writer.Write(InstanceID);

            InstanceOffset.Write(writer);

            if (BaseVersion < 4)
            {
                return;
            }

            writer.Write(JointIndex);

            writer.Write((byte)1);
            writer.Write(JointName);
            writer.Write(new byte[0x40 - JointName.Length - 1]);
        }
    }

    public class LVDMetaInfo
    {
        [ReadOnly(true)]
        public byte Version { get; internal set; } = 1;

        [ReadOnly(true), TypeConverter(typeof(ExpandableObjectConverter))]
        public LVDVersionInfo VersionInfo { get; internal set; } = new LVDVersionInfo();

        public string Name { get; set; }
        
        public void Read(BinaryReaderExt reader)
        {
            Version = reader.ReadByte();

            VersionInfo.Read(reader);

            reader.Skip(1);
            Name = reader.ReadString();
            reader.Skip(0x38 - (uint)Name.Length - 1);
        }

        public void Write(BinaryWriterExt writer)
        {
            writer.Write(Version);

            VersionInfo.Write(writer);

            writer.Write((byte)1);
            writer.Write(Name);
            writer.Write(new byte[0x38 - Name.Length - 1]);
        }
    }

    public class LVDVersionInfo
    {
        [ReadOnly(true)]
        public byte Version { get; internal set; } = 1;

        [ReadOnly(true)]
        public uint EditorVersion { get; internal set; }

        [ReadOnly(true)]
        public uint FormatVersion { get; internal set; }

        public void Read(BinaryReaderExt reader)
        {
            Version = reader.ReadByte();
            EditorVersion = reader.ReadUInt32();
            FormatVersion = reader.ReadUInt32();
        }

        public void Write(BinaryWriterExt writer)
        {
            writer.Write(Version);
            writer.Write(EditorVersion);
            writer.Write(FormatVersion);
        }
    }
}
