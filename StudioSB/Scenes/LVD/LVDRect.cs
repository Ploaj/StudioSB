using StudioSB.Tools;
using System.ComponentModel;

namespace StudioSB.Scenes.LVD
{
    public class LVDRect
    {
        [ReadOnly(true), Category("Version")]
        public byte Version { get; internal set; } = 1;

        [Category("Rectangle")]
        public float Left { get; set; }

        [Category("Rectangle")]
        public float Right { get; set; }

        [Category("Rectangle")]
        public float Top { get; set; }

        [Category("Rectangle")]
        public float Bottom { get; set; }

        public override string ToString()
        {
            return $"({Left}, {Right}, {Top}, {Bottom})";
        }
        public void Read(BinaryReaderExt reader)
        {
            Version = reader.ReadByte();
            Left = reader.ReadSingle();
            Right = reader.ReadSingle();
            Top = reader.ReadSingle();
            Bottom = reader.ReadSingle();
        }

        public void Write(BinaryWriterExt writer)
        {
            writer.Write(Version);
            writer.Write(Left);
            writer.Write(Right);
            writer.Write(Top);
            writer.Write(Bottom);
        }
    }
}
