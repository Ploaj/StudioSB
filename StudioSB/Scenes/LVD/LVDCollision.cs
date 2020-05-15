using StudioSB.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace StudioSB.Scenes.LVD
{
    public class LVDVector2
    {
        public float X { get; set; }

        public float Y { get; set; }

        public LVDVector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        public LVDVector2 Normalized()
        {
            float length = (float)Math.Sqrt(X * X + Y * Y);
            return new LVDVector2(X / length, Y / length);
        }

        public static LVDVector2 GenerateNormal(LVDVector2 v1, LVDVector2 v2)
        {
            LVDVector2 normal = new LVDVector2(v2.Y - v1.Y, v2.X - v1.X).Normalized();
            normal.X *= -1;
            return normal;
        }
    }

    public class LVDVector3
    {
        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public LVDVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }
    }

    public class LVDCollision : LVDEntry
    {
        [Category("Collision")]
        public bool Flag1 { get; set; } = false;

        [Category("Collision")]
        public bool Rigged { get; set; } = false;

        [Category("Collision")]
        public bool Flag3 { get; set; } = false;

        [Category("Collision")]
        public bool PassThrough { get; set; } = false;
        
        [Category("Collision")]
        public List<LVDVector2> Vertices { get; set; } = new List<LVDVector2>();

        [Category("Collision")]
        public List<LVDVector2> Normals { get; set; } = new List<LVDVector2>();

        [Category("Collision")]
        public List<LVDCollisionCliff> Cliffs { get; set; } = new List<LVDCollisionCliff>();

        [Category("Collision")]
        public List<LVDCollisionMaterial> Materials { get; set; } = new List<LVDCollisionMaterial>();
        
        [Category("Collision")]
        public List<LVDCollisionCurve> Curves = new List<LVDCollisionCurve>();

        public void Read(BinaryReaderExt r, int VersionMinor)
        {
            base.Read(r);

            Flag1 = r.ReadBoolean();
            Rigged = r.ReadBoolean();
            Flag3 = r.ReadBoolean();
            PassThrough = r.ReadBoolean();

            r.ReadByte();
            int vertCount = r.ReadInt32();
            for (int i = 0; i < vertCount; i++)
            {
                r.ReadByte();
                Vertices.Add(new LVDVector2(r.ReadSingle(), r.ReadSingle()));
            }
            
            r.ReadByte();
            int normalCount = r.ReadInt32();
            for (int i = 0; i < normalCount; i++)
            {
                r.ReadByte();
                Normals.Add(new LVDVector2(r.ReadSingle(), r.ReadSingle()));
            }

            r.ReadByte();
            int cliffCount = r.ReadInt32();
            for (int i = 0; i < cliffCount; i++)
            {
                var cliff = new LVDCollisionCliff();
                cliff.Read(r);
                Cliffs.Add(cliff);
            }
            
            r.ReadByte();
            int materialCount = r.ReadInt32();
            for (int i = 0; i < materialCount; i++)
            {
                var material = new LVDCollisionMaterial();
                material.Read(r);
                Materials.Add(material);
            }

            // Ultimate Only?

            if(VersionMinor > 10)
            {
                r.ReadByte();
                var vecCount = r.ReadInt32();
                for(int i = 0; i < vecCount; i++)
                {
                    var vec = new LVDCollisionCurve();
                    vec.Read(r);
                    Curves.Add(vec);
                }
            }
        }

        public void Write(BinaryWriterExt writer, int VersionMinor)
        {
            base.Write(writer);

            writer.Write(Flag1);
            writer.Write(Rigged);
            writer.Write(Flag3);
            writer.Write(PassThrough);

            writer.Write((byte)1);
            writer.Write(Vertices.Count);
            foreach(var v in Vertices)
            {
                writer.Write((byte)1);
                writer.Write(v.X);
                writer.Write(v.Y);
            }

            writer.Write((byte)1);
            writer.Write(Normals.Count);
            foreach (var v in Normals)
            {
                writer.Write((byte)1);
                writer.Write(v.X);
                writer.Write(v.Y);
            }
            
            writer.Write((byte)1);
            writer.Write(Cliffs.Count);
            foreach (var v in Cliffs)
            {
                v.Write(writer);
            }

            writer.Write((byte)1);
            writer.Write(Materials.Count);
            foreach (var v in Materials)
            {
                v.Write(writer);
            }
            
            if (VersionMinor > 10)
            {
                writer.Write((byte)1);
                writer.Write(Curves.Count);
                foreach (var v in Curves)
                {
                    v.Write(writer);
                }
            }
        }
    }
}
