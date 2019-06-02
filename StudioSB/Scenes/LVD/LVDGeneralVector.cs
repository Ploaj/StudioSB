using StudioSB.Tools;

namespace StudioSB.Scenes.LVD
{
    public class LVDGeneralVector : LVDEntry
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public override void Read(BinaryReaderExt r)
        {
            base.Read(r);

            r.ReadByte();

            X = r.ReadSingle();
            Y = r.ReadSingle();
            Z = r.ReadSingle();
        }
    }
}
