using System;
using System.Collections.Generic;
using StudioSB.Tools;

namespace StudioSB.Scenes.LVD
{
    public class LVDEnemyGenerator : LVDEntry
    {
        public List<LVDShape> Spawns { get; set; } = new List<LVDShape>();
        public List<LVDShape> Zones { get; set; } = new List<LVDShape>();
        public int Unknown1 { get; set; } = 0;
        public int ID { get; set; }
        public List<int> SpawnIDs { get; set; } = new List<int>();
        public int Unknown2 { get; set; } = 0;
        public List<int> ZoneIDs { get; set; } = new List<int>();
        
        public override void Read(BinaryReaderExt r)
        {
            base.Read(r);

            r.Skip(0x2); //x01 01
            int spawnCount = r.ReadInt32();
            for (int i = 0; i < spawnCount; i++)
            {
                r.Skip(1);
                var shape = new LVDShape();
                shape.Read(r);
                Spawns.Add(shape);
            }

            r.Skip(0x2); //x01 01
            int zoneCount = r.ReadInt32();
            for (int i = 0; i < zoneCount; i++)
            {
                r.Skip(1);
                var shape = new LVDShape();
                shape.Read(r);
                Zones.Add(shape);
            }

            r.Skip(0x2); //x01 01
            Unknown1 = r.ReadInt32() ; //Only seen as 0

            r.Skip(1); //x01
            ID = r.ReadInt32();

            r.Skip(1); //x01
            int spawnIdCount = r.ReadInt32();
            for (int i = 0; i < spawnIdCount; i++)
            {
                r.Skip(1);
                SpawnIDs.Add(r.ReadInt32());
            }

            r.Skip(1); //x01
            Unknown2 = r.ReadInt32(); //Only seen as 0

            r.Skip(1); //x01
            int zoneIdCount = r.ReadInt32();
            for (int i = 0; i < zoneIdCount; i++)
            {
                r.Skip(1);
                ZoneIDs.Add(r.ReadInt32());
            }
        }

        public override void Write(BinaryWriterExt writer)
        {
            base.Write(writer);

            writer.Write((byte)1);
            writer.Write((byte)1);
            writer.Write(Spawns.Count);
            foreach (var v in Spawns)
            {
                writer.Write((byte)1);
                v.Write(writer);
            }

            writer.Write((byte)1);
            writer.Write((byte)1);
            writer.Write(Zones.Count);
            foreach (var v in Zones)
            {
                writer.Write((byte)1);
                v.Write(writer);
            }

            writer.Write((byte)1);
            writer.Write((byte)1);
            writer.Write(0);
            
            writer.Write((byte)1);
            writer.Write(ID);
            
            writer.Write((byte)1);
            writer.Write(SpawnIDs.Count);
            foreach (var v in SpawnIDs)
            {
                writer.Write((byte)1);
                writer.Write(v);
            }

            writer.Write((byte)1);
            writer.Write(0);
            
            writer.Write((byte)1);
            writer.Write(ZoneIDs.Count);
            foreach (var v in ZoneIDs)
            {
                writer.Write((byte)1);
                writer.Write(v);
            }
        }
    }
}
