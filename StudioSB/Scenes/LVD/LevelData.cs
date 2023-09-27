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
        [ReadOnly(true), Category("Header")]
        public uint Unknown { get; internal set; } = 1;

        [ReadOnly(true), Category("Header")]
        public byte Version { get; internal set; } = 13;

        [ReadOnly(true), Category("Header")]
        public string Signature { get; internal set; } = "LVD1";

        public List<LVDCollision> Collisions = new List<LVDCollision>();
        public List<LVDPoint> StartPositions = new List<LVDPoint>();
        public List<LVDPoint> RestartPositions = new List<LVDPoint>();
        public List<LVDRegion> CameraRegions = new List<LVDRegion>();
        public List<LVDRegion> DeathRegions = new List<LVDRegion>();
        public List<LVDEnemyGenerator> EnemyGenerators = new List<LVDEnemyGenerator>();
        public List<LVDDamageShape> DamageShapes = new List<LVDDamageShape>();
        public List<LVDItemPopupRegion> ItemPopupRegions = new List<LVDItemPopupRegion>();
        public List<LVDPTrainerRange> PTrainerRanges = new List<LVDPTrainerRange>();
        public List<LVDPTrainerFloatingFloor> PTrainerFloatingFloors = new List<LVDPTrainerFloatingFloor>();
        public List<LVDGeneralShape2> GeneralShapes2D = new List<LVDGeneralShape2>();
        public List<LVDGeneralShape3> GeneralShapes3D = new List<LVDGeneralShape3>();
        public List<LVDRegion> ShrinkedCameraRegions = new List<LVDRegion>();
        public List<LVDRegion> ShrinkedDeathRegions = new List<LVDRegion>();

        public LevelData()
        {
        }

        public LevelData(string FileName)
        {
            Open(FileName);
        }

        public void Open(string FileName)
        {
            using (BinaryReaderExt stream = new BinaryReaderExt(new FileStream(FileName, FileMode.Open)))
            {
                Read(stream);

                if (stream.BaseStream.Length != stream.BaseStream.Position)
                {
                    stream.PrintPosition();
                    throw new Exception("Error fully parsing LVD file " + stream.BaseStream.Position.ToString("X"));
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
                Write(writer);
            }

            byte[] output = stream.ToArray();
            stream.Close();
            stream.Dispose();
            return output;
        }

        public void Read(BinaryReaderExt reader)
        {
            reader.BigEndian = true;

            Unknown = reader.ReadUInt32();
            Version = reader.ReadByte();

            reader.ReadByte();
            Signature = new string(reader.ReadChars(Signature.Length));

            reader.ReadByte();
            uint collisionCount = reader.ReadUInt32();
            for (uint i = 0; i < collisionCount; i++)
            {
                LVDCollision collision = new LVDCollision();

                collision.Read(reader);
                Collisions.Add(collision);
            }

            reader.ReadByte();
            uint startPositionCount = reader.ReadUInt32();
            for (uint i = 0; i < startPositionCount; i++)
            {
                LVDPoint startPosition = new LVDPoint();

                startPosition.Read(reader);
                StartPositions.Add(startPosition);
            }

            reader.ReadByte();
            uint restartPositionCount = reader.ReadUInt32();
            for (uint i = 0; i < restartPositionCount; i++)
            {
                LVDPoint restartPosition = new LVDPoint();

                restartPosition.Read(reader);
                RestartPositions.Add(restartPosition);
            }

            reader.ReadByte();
            uint cameraRegionCount = reader.ReadUInt32();
            for (uint i = 0; i < cameraRegionCount; i++)
            {
                LVDRegion cameraRegion = new LVDRegion();

                cameraRegion.Read(reader);
                CameraRegions.Add(cameraRegion);
            }

            reader.ReadByte();
            uint deathRegionCount = reader.ReadUInt32();
            for (uint i = 0; i < deathRegionCount; i++)
            {
                LVDRegion deathRegion = new LVDRegion();

                deathRegion.Read(reader);
                DeathRegions.Add(deathRegion);
            }

            reader.ReadByte();
            uint enemyGeneratorCount = reader.ReadUInt32();
            for (uint i = 0; i < enemyGeneratorCount; i++)
            {
                LVDEnemyGenerator enemyGenerator = new LVDEnemyGenerator();

                enemyGenerator.Read(reader);
                EnemyGenerators.Add(enemyGenerator);
            }

            if (Version < 2)
            {
                return;
            }

            reader.ReadByte();
            if (reader.ReadUInt32() > 0)
            {
                throw new NotImplementedException("Unknown LVD Section at 0x" + reader.BaseStream.Position.ToString("X"));
            }

            if (Version < 3)
            {
                return;
            }

            reader.ReadByte();
            if (reader.ReadUInt32() > 0)
            {
                throw new NotImplementedException("Unknown LVD Section at 0x" + reader.BaseStream.Position.ToString("X"));
            }

            reader.ReadByte();
            if (reader.ReadUInt32() > 0)
            {
                throw new NotImplementedException("Unknown LVD Section at 0x" + reader.BaseStream.Position.ToString("X"));
            }

            reader.ReadByte();
            if (reader.ReadUInt32() > 0)
            {
                throw new NotImplementedException("Unknown LVD Section at 0x" + reader.BaseStream.Position.ToString("X"));
            }

            reader.ReadByte();
            if (reader.ReadUInt32() > 0)
            {
                throw new NotImplementedException("Unknown LVD Section at 0x" + reader.BaseStream.Position.ToString("X"));
            }

            if (Version < 4)
            {
                return;
            }

            reader.ReadByte();
            uint damageShapeCount = reader.ReadUInt32();
            for (uint i = 0; i < damageShapeCount; i++)
            {
                LVDDamageShape damageShape = new LVDDamageShape();

                damageShape.Read(reader);
                DamageShapes.Add(damageShape);
            }

            if (Version < 5)
            {
                return;
            }

            reader.ReadByte();
            uint itemPopupRegionCount = reader.ReadUInt32();
            for (uint i = 0; i < itemPopupRegionCount; i++)
            {
                LVDItemPopupRegion itemPopupRegion = new LVDItemPopupRegion();

                itemPopupRegion.Read(reader);
                ItemPopupRegions.Add(itemPopupRegion);
            }

            if (Version > 11)
            {
                reader.ReadByte();
                uint ptrainerRangeCount = reader.ReadUInt32();
                for (uint i = 0; i < ptrainerRangeCount; i++)
                {
                    LVDPTrainerRange ptrainerRange = new LVDPTrainerRange();

                    ptrainerRange.Read(reader);
                    PTrainerRanges.Add(ptrainerRange);
                }

                if (Version > 12)
                {
                    reader.ReadByte();
                    uint ptrainerFloatingFloorCount = reader.ReadUInt32();
                    for (uint i = 0; i < ptrainerFloatingFloorCount; i++)
                    {
                        LVDPTrainerFloatingFloor ptrainerFloatingFloor = new LVDPTrainerFloatingFloor();

                        ptrainerFloatingFloor.Read(reader);
                        PTrainerFloatingFloors.Add(ptrainerFloatingFloor);
                    }
                }
            }

            if (Version < 6)
            {
                return;
            }

            reader.ReadByte();
            uint generalShape2Count = reader.ReadUInt32();
            for (uint i = 0; i < generalShape2Count; i++)
            {
                LVDGeneralShape2 generalShape2 = new LVDGeneralShape2();

                generalShape2.Read(reader);
                GeneralShapes2D.Add(generalShape2);
            }

            reader.ReadByte();
            uint generalShape3Count = reader.ReadUInt32();
            for (uint i = 0; i < generalShape3Count; i++)
            {
                LVDGeneralShape3 generalShape3 = new LVDGeneralShape3();

                generalShape3.Read(reader);
                GeneralShapes3D.Add(generalShape3);
            }

            if (Version < 7)
            {
                return;
            }

            reader.ReadByte();
            if (reader.ReadUInt32() > 0)
            {
                throw new NotImplementedException("Unknown LVD Section at 0x" + reader.BaseStream.Position.ToString("X"));
            }

            if (Version < 8)
            {
                return;
            }

            reader.ReadByte();
            if (reader.ReadUInt32() > 0)
            {
                throw new NotImplementedException("Unknown LVD Section at 0x" + reader.BaseStream.Position.ToString("X"));
            }

            if (Version < 9)
            {
                return;
            }

            reader.ReadByte();
            if (reader.ReadUInt32() > 0)
            {
                throw new NotImplementedException("Unknown LVD Section at 0x" + reader.BaseStream.Position.ToString("X"));
            }

            if (Version < 10)
            {
                return;
            }

            reader.ReadByte();
            if (reader.ReadUInt32() > 0)
            {
                throw new NotImplementedException("Unknown LVD Section at 0x" + reader.BaseStream.Position.ToString("X"));
            }

            if (Version < 11)
            {
                return;
            }

            reader.ReadByte();
            uint shrinkedCameraRegionCount = reader.ReadUInt32();
            for (uint i = 0; i < shrinkedCameraRegionCount; i++)
            {
                LVDRegion shrinkedCameraRegion = new LVDRegion();

                shrinkedCameraRegion.Read(reader);
                ShrinkedCameraRegions.Add(shrinkedCameraRegion);
            }

            reader.ReadByte();
            uint shrinkedDeathRegionCount = reader.ReadUInt32();
            for (uint i = 0; i < shrinkedDeathRegionCount; i++)
            {
                LVDRegion shrinkedDeathRegion = new LVDRegion();

                shrinkedDeathRegion.Read(reader);
                ShrinkedDeathRegions.Add(shrinkedDeathRegion);
            }
        }

        public void Write(BinaryWriterExt writer)
        {
            writer.BigEndian = true;

            writer.Write(Unknown);
            writer.Write(Version);

            writer.Write((byte)1);
            writer.Write(Signature.ToCharArray());

            writer.Write((byte)1);
            writer.Write(Collisions.Count);
            foreach (var v in Collisions)
            {
                v.Write(writer);
            }

            writer.Write((byte)1);
            writer.Write(StartPositions.Count);
            foreach (var v in StartPositions)
            {
                v.Write(writer);
            }

            writer.Write((byte)1);
            writer.Write(RestartPositions.Count);
            foreach (var v in RestartPositions)
            {
                v.Write(writer);
            }

            writer.Write((byte)1);
            writer.Write(CameraRegions.Count);
            foreach (var v in CameraRegions)
            {
                v.Write(writer);
            }

            writer.Write((byte)1);
            writer.Write(DeathRegions.Count);
            foreach (var v in DeathRegions)
            {
                v.Write(writer);
            }

            writer.Write((byte)1);
            writer.Write(EnemyGenerators.Count);
            foreach (var v in EnemyGenerators)
            {
                v.Write(writer);
            }

            if (Version < 2)
            {
                return;
            }

            writer.Write((byte)1);
            writer.Write(0);

            if (Version < 3)
            {
                return;
            }

            writer.Write((byte)1);
            writer.Write(0);

            writer.Write((byte)1);
            writer.Write(0);

            writer.Write((byte)1);
            writer.Write(0);

            writer.Write((byte)1);
            writer.Write(0);

            if (Version < 4)
            {
                return;
            }

            writer.Write((byte)1);
            writer.Write(DamageShapes.Count);
            foreach (var v in DamageShapes)
            {
                v.Write(writer);
            }

            if (Version < 5)
            {
                return;
            }

            writer.Write((byte)1);
            writer.Write(ItemPopupRegions.Count);
            foreach (var v in ItemPopupRegions)
            {
                v.Write(writer);
            }

            if (Version > 11)
            {
                writer.Write((byte)1);
                writer.Write(PTrainerRanges.Count);
                foreach (var v in PTrainerRanges)
                {
                    v.Write(writer);
                }

                if (Version > 12)
                {
                    writer.Write((byte)1);
                    writer.Write(PTrainerFloatingFloors.Count);
                    foreach (var v in PTrainerFloatingFloors)
                    {
                        v.Write(writer);
                    }
                }
            }

            if (Version < 6)
            {
                return;
            }

            writer.Write((byte)1);
            writer.Write(GeneralShapes2D.Count);
            foreach (var v in GeneralShapes2D)
            {
                v.Write(writer);
            }

            writer.Write((byte)1);
            writer.Write(GeneralShapes3D.Count);
            foreach (var v in GeneralShapes3D)
            {
                v.Write(writer);
            }

            if (Version < 7)
            {
                return;
            }

            writer.Write((byte)1);
            writer.Write(0);

            if (Version < 8)
            {
                return;
            }

            writer.Write((byte)1);
            writer.Write(0);

            if (Version < 9)
            {
                return;
            }

            writer.Write((byte)1);
            writer.Write(0);

            if (Version < 10)
            {
                return;
            }

            writer.Write((byte)1);
            writer.Write(0);

            if (Version < 11)
            {
                return;
            }

            writer.Write((byte)1);
            writer.Write(ShrinkedCameraRegions.Count);
            foreach (var v in ShrinkedCameraRegions)
            {
                v.Write(writer);
            }

            writer.Write((byte)1);
            writer.Write(ShrinkedDeathRegions.Count);
            foreach (var v in ShrinkedDeathRegions)
            {
                v.Write(writer);
            }
        }
    }
}
