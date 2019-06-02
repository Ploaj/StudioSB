using System;
using StudioSB.Tools;

namespace StudioSB.Scenes.LVD
{
    public enum LVDDamageShapeType
    {
        Sphere = 2,
        Capsule = 3
    }

    public class LVDDamageShape : LVDEntry
    {
        public LVDDamageShapeType Type { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Dx { get; set; }
        public float Dy { get; set; }
        public float Dz { get; set; }
        public float Radius { get; set; }
        public byte Unknown1 { get; set; }
        public int Unknown2 { get; set; }

        public override void Read(BinaryReaderExt r)
        {
            base.Read(r);

            r.Skip(1);
            Type = (LVDDamageShapeType)r.ReadInt32();
            if (!Enum.IsDefined(typeof(LVDDamageShapeType), Type))
                throw new NotImplementedException($"Unknown damage shape type {Type} at offset {r.BaseStream.Position - 4}");

            X = r.ReadSingle();
            Y = r.ReadSingle();
            Z = r.ReadSingle();
            if (Type == LVDDamageShapeType.Sphere)
            {
                Radius = r.ReadSingle();
                Dx = r.ReadSingle();
                Dy = r.ReadSingle();
                Dz = r.ReadSingle();
            }
            if (Type == LVDDamageShapeType.Capsule)
            {
                Dx = r.ReadSingle();
                Dy = r.ReadSingle();
                Dz = r.ReadSingle();
                Radius = r.ReadSingle();
            }
            Unknown1 = r.ReadByte();
            Unknown2 = r.ReadInt32();
        }
    }
}
