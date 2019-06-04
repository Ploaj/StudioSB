using StudioSB.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace StudioSB.Scenes.LVD
{
    [Serializable]
    public class LevelData
    {
        [ReadOnly(true)]
        public int Heading { get; internal set; } = 0x01;

        [ReadOnly(true)]
        public byte VersionMinor { get; internal set; } = 0x0D;

        [ReadOnly(true)]
        public byte VersionMajor { get; internal set; } = 0x01;

        [ReadOnly(true)]
        public string Magic { get; internal set; } = "LVD1";

        public List<LVDCollision> Collisions = new List<LVDCollision>();

        public List<LVDSpawn> Spawns = new List<LVDSpawn>();

        public List<LVDSpawn> Respawns = new List<LVDSpawn>();

        public List<LVDBounds> CameraBounds = new List<LVDBounds>();

        public List<LVDBounds> BlastZoneBounds = new List<LVDBounds>();

        public List<LVDEnemyGenerator> EnemyGenerators = new List<LVDEnemyGenerator>();

        public List<LVDItemSpawner> ItemSpawners = new List<LVDItemSpawner>();

        public List<LVDDamageShape> DamageShapes = new List<LVDDamageShape>();

        public List<LVDGeneralPoint> GeneralPoints = new List<LVDGeneralPoint>();

        public List<LVDGeneralShape> GeneralShapes = new List<LVDGeneralShape>();

        // version > 10 ie Smash Ultimate

        public List<LVDRangeCurve> RangeCurves = new List<LVDRangeCurve>();

        public List<LVDGeneralVector> GeneralVectors = new List<LVDGeneralVector>();

        public List<LVDBounds> ShrunkCameraBounds = new List<LVDBounds>();

        public List<LVDBounds> ShrunkBlastZoneBounds = new List<LVDBounds>();

        public LevelData()
        {
            //TODO: make defaults?
        }

        public LevelData(string FileName)
        {
            Open(FileName);
        }

        public void Open(string FileName)
        {
            using (BinaryReaderExt stream = new BinaryReaderExt(new FileStream(FileName, FileMode.Open)))
            {
                stream.BigEndian = true;

                Heading = stream.ReadInt32();
                VersionMinor = stream.ReadByte();
                VersionMajor = stream.ReadByte();
                Magic = new string(stream.ReadChars(4));

                stream.ReadByte();
                var collisionCount = stream.ReadInt32();
                for(int i = 0; i < collisionCount; i++)
                {
                    LVDCollision col = new LVDCollision();
                    col.Read(stream, VersionMinor);
                    Collisions.Add(col);
                }

                stream.ReadByte();
                var spawnCount = stream.ReadInt32();
                for (int i = 0; i < spawnCount; i++)
                {
                    LVDSpawn spawn = new LVDSpawn();
                    spawn.Read(stream);
                    Spawns.Add(spawn);
                }

                stream.ReadByte();
                var respawnCount = stream.ReadInt32();
                for (int i = 0; i < respawnCount; i++)
                {
                    LVDSpawn respawn = new LVDSpawn();
                    respawn.Read(stream);
                    Respawns.Add(respawn);
                }
                
                stream.ReadByte();
                var boundsCount = stream.ReadInt32();
                for (int i = 0; i < boundsCount; i++)
                {
                    LVDBounds bound = new LVDBounds();
                    bound.Read(stream);
                    CameraBounds.Add(bound);
                }

                stream.ReadByte();
                var blastCount = stream.ReadInt32();
                for (int i = 0; i < blastCount; i++)
                {
                    LVDBounds blast = new LVDBounds();
                    blast.Read(stream);
                    BlastZoneBounds.Add(blast);
                }
                
                stream.ReadByte();
                int enemyGenCount = stream.ReadInt32();
                for(int i = 0; i < enemyGenCount; i++)
                {
                    LVDEnemyGenerator enGen = new LVDEnemyGenerator();
                    enGen.Read(stream);
                    EnemyGenerators.Add(enGen);
                }

                stream.ReadByte();
                if (stream.ReadInt32() > 0)
                    throw new NotImplementedException("Unknown LVD Section at 0x" + stream.BaseStream.Position.ToString("X"));

                stream.ReadByte();
                if (stream.ReadInt32() > 0)
                    throw new NotImplementedException("Unknown LVD Section at 0x" + stream.BaseStream.Position.ToString("X"));

                stream.ReadByte();
                if (stream.ReadInt32() > 0)
                    throw new NotImplementedException("Unknown LVD Section at 0x" + stream.BaseStream.Position.ToString("X"));

                stream.ReadByte();
                if (stream.ReadInt32() > 0)
                    throw new NotImplementedException("Unknown LVD Section at 0x" + stream.BaseStream.Position.ToString("X"));

                stream.ReadByte();
                if (stream.ReadInt32() > 0)
                    throw new NotImplementedException("Unknown LVD Section at 0x" + stream.BaseStream.Position.ToString("X"));

                stream.ReadByte();
                int damageCount = stream.ReadInt32();
                for (int i = 0; i < damageCount; i++)
                {
                    LVDDamageShape shape = new LVDDamageShape();
                    shape.Read(stream);
                    DamageShapes.Add(shape);
                }
                
                stream.ReadByte();
                int itemSpawnCount = stream.ReadInt32();
                for (int i = 0; i < itemSpawnCount; i++)
                {
                    LVDItemSpawner spawn = new LVDItemSpawner();
                    spawn.Read(stream);
                    ItemSpawners.Add(spawn);
                }
                
                if (VersionMinor > 0xA)
                {
                    stream.ReadByte();
                    int genCurveCount = stream.ReadInt32();
                    for (int i = 0; i < genCurveCount; i++)
                    {
                        LVDRangeCurve point = new LVDRangeCurve();
                        point.Read(stream);
                        RangeCurves.Add(point);
                    }
                    
                    stream.ReadByte();
                    int genCurveCount2 = stream.ReadInt32();
                    for (int i = 0; i < genCurveCount2; i++)
                    {
                        LVDGeneralVector point = new LVDGeneralVector();
                        point.Read(stream);
                        GeneralVectors.Add(point);
                    }
                }
                
                stream.ReadByte();
                int genShapeCount = stream.ReadInt32();
                for (int i = 0; i < genShapeCount; i++)
                {
                    LVDGeneralShape shape = new LVDGeneralShape();
                    shape.Read(stream);
                    GeneralShapes.Add(shape);
                }
                
                stream.ReadByte();
                int genPointCount = stream.ReadInt32();
                for (int i = 0; i < genPointCount; i++)
                {
                    LVDGeneralPoint point = new LVDGeneralPoint();
                    point.Read(stream);
                    GeneralPoints.Add(point);
                }
                
                stream.ReadByte();
                if (stream.ReadInt32() > 0)
                    throw new NotImplementedException("Unknown LVD Section at 0x" + stream.BaseStream.Position.ToString("X"));

                stream.ReadByte();
                if (stream.ReadInt32() > 0)
                    throw new NotImplementedException("Unknown LVD Section at 0x" + stream.BaseStream.Position.ToString("X"));

                stream.ReadByte();
                if (stream.ReadInt32() > 0)
                    throw new NotImplementedException("Unknown LVD Section at 0x" + stream.BaseStream.Position.ToString("X"));

                stream.ReadByte();
                if (stream.ReadInt32() > 0)
                    throw new NotImplementedException("Unknown LVD Section at 0x" + stream.BaseStream.Position.ToString("X"));
                
                if(VersionMinor > 0xA)
                {
                    stream.ReadByte();
                    var shrunkboundsCount = stream.ReadInt32();
                    for (int i = 0; i < shrunkboundsCount; i++)
                    {
                        LVDBounds bound = new LVDBounds();
                        bound.Read(stream);
                        ShrunkCameraBounds.Add(bound);
                    }

                    stream.ReadByte();
                    var shrunkblastCount = stream.ReadInt32();
                    for (int i = 0; i < shrunkblastCount; i++)
                    {
                        LVDBounds blast = new LVDBounds();
                        blast.Read(stream);
                        ShrunkBlastZoneBounds.Add(blast);
                    }
                }

                if(stream.BaseStream.Length != stream.BaseStream.Position)
                {
                    stream.PrintPosition();
                    throw new Exception("Error fully parsing LVD " + stream.BaseStream.Position.ToString("X"));
                }
            }
        }

        public void Save(string FileName)
        {
            File.WriteAllBytes(FileName, GetData());
        }

        public byte[] GetData()
        {
            MemoryStream stream = new MemoryStream();

            using (BinaryWriterExt writer = new BinaryWriterExt(stream))
            {
                writer.BigEndian = true;

                writer.Write(Heading);
                writer.Write(VersionMinor);
                writer.Write(VersionMajor);
                writer.Write(Magic.ToCharArray());

                writer.Write((byte)1);
                writer.Write(Collisions.Count);
                foreach (var v in Collisions)
                    v.Write(writer, VersionMinor);

                writer.Write((byte)1);
                writer.Write(Spawns.Count);
                foreach (var v in Spawns)
                    v.Write(writer);

                writer.Write((byte)1);
                writer.Write(Respawns.Count);
                foreach (var v in Respawns)
                    v.Write(writer);

                writer.Write((byte)1);
                writer.Write(CameraBounds.Count);
                foreach (var v in CameraBounds)
                    v.Write(writer);

                writer.Write((byte)1);
                writer.Write(BlastZoneBounds.Count);
                foreach (var v in BlastZoneBounds)
                    v.Write(writer);

                writer.Write((byte)1);
                writer.Write(EnemyGenerators.Count);
                foreach (var v in EnemyGenerators)
                    v.Write(writer);

                writer.Write((byte)1);
                writer.Write(0);

                writer.Write((byte)1);
                writer.Write(0);

                writer.Write((byte)1);
                writer.Write(0);

                writer.Write((byte)1);
                writer.Write(0);

                writer.Write((byte)1);
                writer.Write(0);

                writer.Write((byte)1);
                writer.Write(DamageShapes.Count);
                foreach (var v in DamageShapes)
                    v.Write(writer);

                writer.Write((byte)1);
                writer.Write(ItemSpawners.Count);
                foreach (var v in ItemSpawners)
                    v.Write(writer);

                if (VersionMinor > 0xA)
                {
                    writer.Write((byte)1);
                    writer.Write(RangeCurves.Count);
                    foreach (var v in RangeCurves)
                        v.Write(writer);
                    
                    writer.Write((byte)1);
                    writer.Write(GeneralVectors.Count);
                    foreach (var v in GeneralVectors)
                        v.Write(writer);
                }

                writer.Write((byte)1);
                writer.Write(GeneralShapes.Count);
                foreach (var v in GeneralShapes)
                    v.Write(writer);

                writer.Write((byte)1);
                writer.Write(GeneralPoints.Count);
                foreach (var v in GeneralPoints)
                    v.Write(writer);

                writer.Write((byte)1);
                writer.Write(0);

                writer.Write((byte)1);
                writer.Write(0);

                writer.Write((byte)1);
                writer.Write(0);

                writer.Write((byte)1);
                writer.Write(0);

                if (VersionMinor > 0xA)
                {
                    writer.Write((byte)1);
                    writer.Write(ShrunkCameraBounds.Count);
                    foreach (var v in ShrunkCameraBounds)
                        v.Write(writer);

                    writer.Write((byte)1);
                    writer.Write(ShrunkBlastZoneBounds.Count);
                    foreach (var v in ShrunkBlastZoneBounds)
                        v.Write(writer);
                }
            }

            byte[] output = stream.ToArray(); ;
            stream.Close();
            stream.Dispose();
            return  output;
        }
    }
}
