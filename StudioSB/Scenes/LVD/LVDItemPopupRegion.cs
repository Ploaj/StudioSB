using StudioSB.Tools;
using System.Collections.Generic;
using System.ComponentModel;

namespace StudioSB.Scenes.LVD
{
    public class LVDItemPopupRegion : LVDBase
    {
        [ReadOnly(true), Category("Version")]
        public byte Version { get; internal set; } = 1;

        [Category("ID")]
        public int Tag { get; set; }

        [Category("Regions")]
        public List<LVDShape2> Regions { get; set; } = new List<LVDShape2>();

        public override void Read(BinaryReaderExt reader)
        {
            Version = reader.ReadByte();

            base.Read(reader);

            reader.Skip(1);
            Tag = reader.ReadInt32();

            reader.Skip(1);
            reader.Skip(1);
            uint regionCount = reader.ReadUInt32();
            for (uint i = 0; i < regionCount; i++)
            {
                reader.Skip(1);
                var region = new LVDShape2();
                region.Read(reader);
                Regions.Add(region);
            }
        }

        public override void Write(BinaryWriterExt writer)
        {
            writer.Write(Version);

            base.Write(writer);

            writer.Write((byte)1);
            writer.Write(Tag);

            writer.Write((byte)1);
            writer.Write((byte)1);
            writer.Write(Regions.Count);
            foreach (var v in Regions)
            {
                writer.Write((byte)1);
                v.Write(writer);
            }
        }
    }
}
