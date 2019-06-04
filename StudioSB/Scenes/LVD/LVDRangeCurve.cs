using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudioSB.Tools;

namespace StudioSB.Scenes.LVD
{
    public class LVDRangeCurve : LVDEntry
    {
        public LVDVector3 Vector1 { get; set; }
        public LVDVector3 Vector2 { get; set; }

        public List<LVDVector3> Vectors { get; set; } = new List<LVDVector3>();

        public float[] Mat4x4_1 { get; set; } = new float[16];

        public float[] Mat4x4_2 { get; set; } = new float[16];

        public override void Read(BinaryReaderExt r)
        {
            base.Read(r);
            r.ReadByte();
            Vector1 = new LVDVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
            r.ReadByte();
            Vector2 = new LVDVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
            r.ReadByte();

            int vec3Count = r.ReadInt32();
            for(int i = 0; i < vec3Count; i++)
            {
                r.ReadByte();
                Vectors.Add(new LVDVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()));
            }
            
            r.ReadByte();
            Mat4x4_1 = new float[16];
            for (int i = 0; i < 16; i++)
                Mat4x4_1[i] = r.ReadSingle();

            r.ReadByte();
            Mat4x4_2 = new float[16];
            for (int i = 0; i < 16; i++)
                Mat4x4_2[i] = r.ReadSingle();
        }

        public override void Write(BinaryWriterExt writer)
        {
            base.Write(writer);

            writer.Write((byte)1);
            writer.Write(Vector1.X);
            writer.Write(Vector1.Y);
            writer.Write(Vector1.Z);

            writer.Write((byte)1);
            writer.Write(Vector2.X);
            writer.Write(Vector2.Y);
            writer.Write(Vector2.Z);

            writer.Write((byte)1);
            writer.Write(Vectors.Count);
            foreach(var v in Vectors)
            {
                writer.Write((byte)1);
                writer.Write(v.X);
                writer.Write(v.Y);
                writer.Write(v.Z);
            }

            writer.Write((byte)1);
            foreach (var v in Mat4x4_1)
                writer.Write(v);

            writer.Write((byte)1);
            foreach (var v in Mat4x4_2)
                writer.Write(v);
        }
    }
}
