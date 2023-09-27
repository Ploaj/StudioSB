using StudioSB.Tools;
using System.Collections.Generic;
using System.ComponentModel;

namespace StudioSB.Scenes.LVD
{
    public enum LVDShape2Type : uint
    {
        Point = 1,
        Circle = 2,
        Rectangle = 3,
        Path = 4
    }

    public class LVDShape2
    {
        [ReadOnly(true), Category("Version")]
        public byte Version { get; internal set; } = 3;

        [Category("Type")]
        public LVDShape2Type Type { get; set; }

        [Category("Values")]
        public float X { get; set; }

        [Category("Values")]
        public float Y { get; set; }

        [Category("Values")]
        public float Z { get; set; }

        [Category("Values")]
        public float W { get; set; }

        [Category("Values (Path)")]
        public List<LVDVector2> Points { get; set; } = new List<LVDVector2>();

        public override string ToString()
        {
            return Type.ToString();
        }

        public void Read(BinaryReaderExt reader)
        {
            Version = reader.ReadByte();
            Type = (LVDShape2Type)reader.ReadUInt32();
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
            Z = reader.ReadSingle();
            W = reader.ReadSingle();

            reader.Skip(1);
            reader.Skip(1);
            uint pointCount = reader.ReadUInt32();
            for (uint i = 0; i < pointCount; i++)
            {
                LVDVector2 point = new LVDVector2(0.0f, 0.0f);

                point.Read(reader);
                Points.Add(point);
            }
        }

        public void Write(BinaryWriterExt writer)
        {
            writer.Write(Version);
            writer.Write((uint)Type);
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
            writer.Write(W);

            writer.Write((byte)1);
            writer.Write((byte)1);
            writer.Write(Points.Count);
            foreach (var v in Points)
            {
                v.Write(writer);
            }
        }
    }
}
