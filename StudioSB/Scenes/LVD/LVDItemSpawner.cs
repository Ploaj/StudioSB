using System.Collections.Generic;
using StudioSB.Tools;

namespace StudioSB.Scenes.LVD
{
    public class LVDItemSpawner : LVDEntry
    {
        public int ID { get; internal set; } = 0x09840001;
        public List<LVDShape> Sections { get; set; } = new List<LVDShape>();

        public override void Read(BinaryReaderExt r)
        {
            base.Read(r);
            
            r.Skip(1);
            ID = r.ReadInt32();

            r.Skip(1);
            r.Skip(1);
            int sectionCount = r.ReadInt32();
            for (int i = 0; i < sectionCount; i++)
            {
                r.Skip(1);
                var shape = new LVDShape();
                shape.Read(r);
                Sections.Add(shape);
            }
        }

        public override void Write(BinaryWriterExt writer)
        {
            base.Write(writer);

            writer.Write((byte)1);
            writer.Write(ID);

            writer.Write((byte)1);
            writer.Write((byte)1);

            writer.Write(Sections.Count);
            foreach(var v in Sections)
            {
                writer.Write((byte)1);
                v.Write(writer);
            }
        }
    }
}
