using StudioSB.Tools;
using System.Collections.Generic;

namespace StudioSB.Scenes.LVD
{
    public enum LVDShapeType : int
    {
        Point = 1,
        Circle = 2,
        Rectangle = 3,
        Path = 4
    }

    public class LVDShape 
    {
        public LVDShapeType Type { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }
        public List<LVDVector2> points = new List<LVDVector2>();

        public void Read(BinaryReaderExt r)
        {
            r.ReadByte();
            Type = (LVDShapeType)r.ReadInt32();
            X = r.ReadSingle();
            Y = r.ReadSingle();
            Z = r.ReadSingle();
            W = r.ReadSingle();

            r.ReadByte();

            r.ReadByte();
            int pointCount = r.ReadInt32();
            for (int i = 0; i < pointCount; i++)
            {
                r.Skip(1);
                points.Add(new LVDVector2(r.ReadSingle(), r.ReadSingle()));
            }
        }

        public void Write(BinaryWriterExt writer)
        {
            writer.Write((byte)3);
            writer.Write((int)Type);
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
            writer.Write(W);

            writer.Write((byte)1);
            writer.Write((byte)1);
            writer.Write(points.Count);
            foreach(var v in points)
            {
                writer.Write((byte)1);
                writer.Write(v.X);
                writer.Write(v.Y);
            }
        }
    }
}
