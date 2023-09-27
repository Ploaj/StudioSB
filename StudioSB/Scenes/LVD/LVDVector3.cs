using StudioSB.Tools;
using System.ComponentModel;

namespace StudioSB.Scenes.LVD
{
    public class LVDVector3
    {
        [ReadOnly(true), Category("Version")]
        public byte Version { get; internal set; } = 1;

        [Category("Component")]
        public float X { get; set; }

        [Category("Component")]
        public float Y { get; set; }

        [Category("Component")]
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

        public void Read(BinaryReaderExt reader)
        {
            Version = reader.ReadByte();
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
            Z = reader.ReadSingle();
        }

        public void Write(BinaryWriterExt writer)
        {
            writer.Write(Version);
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
        }
    }
}
