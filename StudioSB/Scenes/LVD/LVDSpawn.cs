using StudioSB.Tools;

namespace StudioSB.Scenes.LVD
{
    public class LVDSpawn : LVDEntry
    {
        public float X { get; set; } = 0;
        public float Y { get; set; } = 0;

        public override void Read(BinaryReaderExt r)
        {
            base.Read(r);

            r.ReadByte();
            X = r.ReadSingle();
            Y = r.ReadSingle();
        }
    }
}
