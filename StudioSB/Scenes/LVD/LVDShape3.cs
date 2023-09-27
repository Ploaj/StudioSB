using StudioSB.Tools;
using System.ComponentModel;

namespace StudioSB.Scenes.LVD
{
    public enum LVDShape3Type : uint
    {
        Box = 1,
        Sphere = 2,
        Capsule = 3,
        Point = 4
    }

    public class LVDShape3
    {
        [ReadOnly(true), Category("Version")]
        public byte Version { get; internal set; } = 1;

        [Category("Type")]
        public LVDShape3Type Type { get; set; }

        [Category("Values")]
        public float X { get; set; }

        [Category("Values")]
        public float Y { get; set; }

        [Category("Values")]
        public float Z { get; set; }

        [Category("Values")]
        public float W { get; set; }

        [Category("Values")]
        public float S { get; set; }

        [Category("Values")]
        public float R { get; set; }

        [Category("Values")]
        public float T { get; set; }

        public override string ToString()
        {
            return Type.ToString();
        }

        public void Read(BinaryReaderExt reader)
        {
            Version = reader.ReadByte();
            Type = (LVDShape3Type)reader.ReadUInt32();
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
            Z = reader.ReadSingle();
            W = reader.ReadSingle();
            S = reader.ReadSingle();
            R = reader.ReadSingle();
            T = reader.ReadSingle();
        }

        public void Write(BinaryWriterExt writer)
        {
            writer.Write(Version);
            writer.Write((uint)Type);
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
            writer.Write(W);
            writer.Write(S);
            writer.Write(R);
            writer.Write(T);
        }
    }
}
