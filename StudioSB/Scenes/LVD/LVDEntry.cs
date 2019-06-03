using OpenTK;
using StudioSB.Tools;
using System;
using System.ComponentModel;

namespace StudioSB.Scenes.LVD
{
    public class LVDEntry
    {
        [ReadOnly(true), Category("Entry")]
        public long EntryFlags { get; internal set; }

        [ReadOnly(true), Category("Entry")]
        public int EntryNumber { get; internal set; }

        [Category("Entry")]
        public string EntryName { get; set; }

        [Category("Entry")]
        public string EntryLabel { get; set; }

        [Category("Entry")]
        public LVDVector3 StartPosition { get; set; } = new LVDVector3(0, 0, 0);

        [Category("Entry")]
        public bool UseStartPosition { get; set; } = false;

        [Category("Entry")]
        public LVDVector3 UnknownVector { get; set; } = new LVDVector3(0, 0, 0);

        [Category("Entry")]
        public int UnknownIndex { get; set; } = 0;

        [Category("Entry")]
        public int UnknownIndex2 { get; set; } = 0; // usually 0; related to connecting to bones? wily has 1 and 2

        [Category("Entry")]
        public string BoneName { get; set; }

        public virtual void Read(BinaryReaderExt r)
        {
            EntryFlags = r.ReadInt64();
            EntryNumber = r.ReadInt32();

            bool hasString = r.ReadBoolean();
            EntryName = r.ReadString(0x38);

            bool hasLabel = r.ReadBoolean();
            EntryLabel = r.ReadString(0x40);

            bool hasStartPosition = r.ReadBoolean();
            StartPosition = new LVDVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
            UseStartPosition = r.ReadBoolean();
            
            // Unknown
            r.Skip(1);
            UnknownIndex2 = r.ReadInt32();
            
            r.Skip(1);
            UnknownVector = new LVDVector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
            UnknownIndex = r.ReadInt32();

            r.Skip(1);
            BoneName = r.ReadString(0x40);
        }

        public virtual void Write()
        {

        }
    }
}
