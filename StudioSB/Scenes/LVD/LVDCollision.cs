using StudioSB.Tools;
using System.Collections.Generic;
using System.ComponentModel;

namespace StudioSB.Scenes.LVD
{
    public class LVDCollision : LVDBase
    {
        [ReadOnly(true), Category("Version")]
        public byte Version { get; internal set; } = 4;

        [Category("CollisionFlags")]
        public bool Dynamic { get; set; } = false;

        [Category("CollisionFlags")]
        public bool DropThrough { get; set; } = false;
        
        [Category("Collision")]
        public List<LVDVector2> Vertices { get; set; } = new List<LVDVector2>();

        [Category("Collision")]
        public List<LVDVector2> Normals { get; set; } = new List<LVDVector2>();

        [Category("Collision")]
        public List<LVDCollisionCliff> Cliffs { get; set; } = new List<LVDCollisionCliff>();

        [Category("Collision")]
        public List<LVDCollisionAttribute> Attributes { get; set; } = new List<LVDCollisionAttribute>();

        [Category("Collision")]
        public List<LVDCollisionSpiritsFloor> SpiritsFloors { get; set; } = new List<LVDCollisionSpiritsFloor>();

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

            reader.Skip(1);
            Dynamic = reader.ReadBoolean();
            reader.Skip(1);
            DropThrough = reader.ReadBoolean();

            reader.Skip(1);
            uint vertexCount = reader.ReadUInt32();
            for (uint i = 0; i < vertexCount; i++)
            {
                LVDVector2 vertex = new LVDVector2(0.0f, 0.0f);

                vertex.Read(reader);
                Vertices.Add(vertex);
            }

            reader.Skip(1);
            uint normalCount = reader.ReadUInt32();
            for (uint i = 0; i < normalCount; i++)
            {
                LVDVector2 normal = new LVDVector2(0.0f, 0.0f);

                normal.Read(reader);
                Normals.Add(normal);
            }

            reader.Skip(1);
            uint cliffCount = reader.ReadUInt32();
            for (uint i = 0; i < cliffCount; i++)
            {
                LVDCollisionCliff cliff = new LVDCollisionCliff();

                cliff.Read(reader);
                Cliffs.Add(cliff);
            }

            if (Version < 3)
            {
                return;
            }

            reader.Skip(1);
            uint attributeCount = reader.ReadUInt32();
            for (uint i = 0; i < attributeCount; i++)
            {
                LVDCollisionAttribute attribute = new LVDCollisionAttribute();

                attribute.Read(reader);
                Attributes.Add(attribute);
            }

            if (Version < 4)
            {
                return;
            }

            reader.Skip(1);
            uint spiritsFloorCount = reader.ReadUInt32();
            for (uint i = 0; i < spiritsFloorCount; i++)
            {
                LVDCollisionSpiritsFloor spiritsFloor = new LVDCollisionSpiritsFloor();

                spiritsFloor.Read(reader);
                SpiritsFloors.Add(spiritsFloor);
            }
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

            writer.Write((byte)0);
            writer.Write(Dynamic);
            writer.Write((byte)0);
            writer.Write(DropThrough);

            writer.Write((byte)1);
            writer.Write(Vertices.Count);
            foreach (var v in Vertices)
            {
                v.Write(writer);
            }

            writer.Write((byte)1);
            writer.Write(Normals.Count);
            foreach (var v in Normals)
            {
                v.Write(writer);
            }
            
            writer.Write((byte)1);
            writer.Write(Cliffs.Count);
            foreach (var v in Cliffs)
            {
                v.Write(writer);
            }

            if (Version < 3)
            {
                return;
            }

            writer.Write((byte)1);
            writer.Write(Attributes.Count);
            foreach (var v in Attributes)
            {
                v.Write(writer);
            }

            if (Version < 4)
            {
                return;
            }

            writer.Write((byte)1);
            writer.Write(SpiritsFloors.Count);
            foreach (var v in SpiritsFloors)
            {
                v.Write(writer);
            }
        }
    }
}
