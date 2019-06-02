using StudioSB.Tools;

namespace StudioSB.Scenes.LVD
{
    public class LVDGeneralPoint : LVDEntry
    {
        public int ID { get; set; }
        public int Type { get; set; } = 4;
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        
        public override void Read(BinaryReaderExt r)
        {
            base.Read(r);

            r.Skip(1);
            ID = r.ReadInt32();

            r.Skip(1);
            Type = r.ReadInt32();

            X = r.ReadSingle();
            Y = r.ReadSingle();
            Z = r.ReadSingle();
            r.Skip(0x10);
        }
    }
}
