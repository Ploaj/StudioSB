using StudioSB.Tools;
using System;
using System.ComponentModel;

namespace StudioSB.Scenes.LVD
{
    public class LVDVector2
    {
        [ReadOnly(true), Category("Version")]
        public byte Version { get; internal set; } = 1;

        [Category("Component")]
        public float X { get; set; }

        [Category("Component")]
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
            normal.X *= -1.0f;

            return normal;
        }

        public void Read(BinaryReaderExt reader)
        {
            Version = reader.ReadByte();
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
        }

        public void Write(BinaryWriterExt writer)
        {
            writer.Write(Version);
            writer.Write(X);
            writer.Write(Y);
        }
    }
}
