using System;
using System.Collections.Generic;
using StudioSB.Scenes;
using StudioSB.Scenes.Animation;
using HSDRaw;
using System.IO;
using OpenTK;
using System.ComponentModel;
using System.Drawing.Design;
using StudioSB.GUI;
using System.Windows.Forms;

namespace StudioSB.IO.Formats
{
    public class IO_8MOT : IImportableAnimation
    {
        public class MOTINGSettings
        {
            [DisplayName("Frame Scale"),
                Description("Scales the frames by this amount")]
            public int FrameScale { get; set; } = 60;

            [Editor(typeof(FilteredFileNameEditor),typeof(UITypeEditor)),
                DisplayName("JCV Path"),
                Description("Joint Connector Variable File")]
            public string JVCPath { get; set; } = "";

            /*[DisplayName("Bake Frames"),
                Description("Bakes out the keyframes to fix rotation error")]
            public bool BakeFrames { get; set; } = true;*/
        }

        private static MOTINGSettings Settings = new MOTINGSettings();

        public string Name => "Eighting Motion Format";

        public string Extension => ".gnta";

        private Vector3 ToEul(Quaternion baseRotation, float angle, Vector3 dir)
        {
            return Tools.CrossMath.ToEulerAngles(ToQuat(baseRotation, angle, dir));
        }

        private Quaternion ToQuat(Quaternion baseRotation, float angle, Vector3 dir)
        {
            Matrix4 final = Matrix4.Identity;

            float rot_angle = (float)Math.Acos(Vector3.Dot(dir, Vector3.UnitX));

            if (Math.Abs(rot_angle) > 0.000001f)
            {
                Vector3 rot_axiz = Vector3.Cross(dir, Vector3.UnitX).Normalized();
                final = Matrix4.CreateFromAxisAngle(rot_axiz, rot_angle);
            }

            Matrix4 xrot;
            Matrix4.CreateRotationX(-angle * (float)Math.PI / 180, out xrot);

            final = Matrix4.CreateFromQuaternion(baseRotation.Inverted()) * (final * xrot);

            return final.ExtractRotation().Inverted();
        }

        public SBAnimation ImportSBAnimation(string FileName, SBSkeleton skeleton)
        {
            var sbAnim = new SBAnimation();

            using (SBCustomDialog d = new SBCustomDialog(Settings))
            {
                d.ShowDialog();
            }

            var jointTable = GetJointTable(Settings.JVCPath);

            var anim = new Anim();
            using (BinaryReaderExt r = new BinaryReaderExt(new FileStream(FileName, FileMode.Open)))
                anim.Parse(r);

            float scale = Settings.FrameScale;
            sbAnim.FrameCount = (float)Math.Ceiling(scale * anim.EndTime);

            foreach (var j in anim.Joints)
            {
                if (j.ID == -1)
                    continue;

                if (j.ID >= jointTable.Length || jointTable[j.ID] >= skeleton.Bones.Length)
                    continue;

                var bone = skeleton.Bones[jointTable[j.ID]];

                SBConsole.WriteLine(bone.Name);

                SBTransformAnimation node = sbAnim.TransformNodes.Find(e => e.Name == bone.Name);
                if (node == null)
                {
                    node = new SBTransformAnimation();
                    node.Name = bone.Name;
                    sbAnim.TransformNodes.Add(node);
                }

                if (j.Flag3 == 0x21)
                {
                    foreach (var k in j.Keys)
                    {
                        node.AddKey((float)Math.Ceiling(k.Time * scale), bone.X + k.X, SBTrackType.TranslateX);
                        node.AddKey((float)Math.Ceiling(k.Time * scale), bone.Y + k.Y, SBTrackType.TranslateY);
                        node.AddKey((float)Math.Ceiling(k.Time * scale), bone.Z + k.Z, SBTrackType.TranslateZ);
                    }
                }
                else
                if (j.Flag3 == 0x28)
                {
                    var eul0 = ToQuat(bone.RotationQuaternion, j.Keys[0].W, new Vector3(j.Keys[0].X, j.Keys[0].Y, j.Keys[0].Z));
                    node.AddKey(0, eul0.X, SBTrackType.RotateX, InterpolationType.Step);
                    node.AddKey(0, eul0.Y, SBTrackType.RotateY, InterpolationType.Step);
                    node.AddKey(0, eul0.Z, SBTrackType.RotateZ, InterpolationType.Step);
                    node.AddKey(0, eul0.W, SBTrackType.RotateW, InterpolationType.Step);
                    foreach (var k in j.Keys)
                    {
                        var eul = ToQuat(bone.RotationQuaternion, k.W, new Vector3(k.X, k.Y, k.Z));

                        node.AddKey((float)Math.Ceiling(k.Time * scale), eul.X, SBTrackType.RotateX);
                        node.AddKey((float)Math.Ceiling(k.Time * scale), eul.Y, SBTrackType.RotateY);
                        node.AddKey((float)Math.Ceiling(k.Time * scale), eul.Z, SBTrackType.RotateZ);
                        node.AddKey((float)Math.Ceiling(k.Time * scale), eul.W, SBTrackType.RotateW);
                    }
                }
                else
                    SBConsole.WriteLine("Unknown MOT Track Type " + j.Flag3.ToString("X"));
            }

            sbAnim.ConvertRotationKeysToEuler();

            return sbAnim;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static short[] GetJointTable(string filePath)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show("No JCV file loaded", "JCV File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return new short[0];
            }

            using (BinaryReaderExt r = new BinaryReaderExt(new FileStream(filePath, FileMode.Open)))
            {
                r.BigEndian = true;

                r.Seek(0x10);
                var count = r.ReadInt16();

                short[] vals = new short[count];

                for (int i = 0; i < vals.Length; i++)
                    vals[i] = r.ReadInt16();

                return vals;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class Anims
        {
            public List<Anim> AnimList = new List<Anim>();
        }   

        /// <summary>
        /// 
        /// </summary>
        public class Anim
        {
            public float StartTime;
            
            public float EndTime;
            
            public List<Joint> Joints = new List<Joint>();

            public void Parse(BinaryReaderExt r)
            {
                r.BigEndian = true;

                var start = r.Position;

                var sectionCount = r.ReadInt32();
                var sectionHeaderLength = r.ReadInt32();
                StartTime = r.ReadSingle();
                EndTime = r.ReadSingle();

                for (int j = 0; j < sectionCount; j++)
                {
                    Joint joint = new Joint();

                    Joints.Add(joint);

                    joint.Flag1 = r.ReadByte();
                    joint.Flag2 = r.ReadByte();
                    joint.Flag3 = r.ReadUInt16();
                    joint.ID = r.ReadInt16();
                    var floatCount = r.ReadInt16();

                    joint.MaxTime = r.ReadSingle();
                    joint.Unknown = r.ReadInt32();

                    var offset1 = r.ReadUInt32() + start;
                    var offset2 = r.ReadUInt32() + start;
                    var offset3 = r.ReadUInt32() + start;
                    var offset4 = r.ReadUInt32() + start;

                    if (offset3 != start)
                        throw new NotSupportedException("Section 3 detected");

                    if (offset4 != start)
                        throw new NotSupportedException("Section 4 detected");

                    var temp = r.Position;
                    for (uint k = 0; k < floatCount; k++)
                    {
                        Key key = new Key();

                        r.Seek(offset1 + 4 * k);
                        key.Time = r.ReadSingle();

                        if (offset2 != start)
                        {
                            r.Seek(offset2 + 8 * k);

                            key.X = BitConverter.ToSingle(BitConverter.GetBytes(((r.ReadByte() & 0xFF) << 24) | ((r.ReadByte() & 0xFF) << 16)), 0);
                            key.Y = BitConverter.ToSingle(BitConverter.GetBytes(((r.ReadByte() & 0xFF) << 24) | ((r.ReadByte() & 0xFF) << 16)), 0);
                            key.Z = BitConverter.ToSingle(BitConverter.GetBytes(((r.ReadByte() & 0xFF) << 24) | ((r.ReadByte() & 0xFF) << 16)), 0);
                            key.W = BitConverter.ToSingle(BitConverter.GetBytes(((r.ReadByte() & 0xFF) << 24) | ((r.ReadByte() & 0xFF) << 16)), 0);
                        }

                        joint.Keys.Add(key);
                    }


                    r.Seek(temp);

                }
            }
        }

        public class Joint
        {
            public byte Flag1;
            
            public byte Flag2;
            
            public ushort Flag3;
            
            public short ID;
        
            public float MaxTime;
            
            public int Unknown;
            
            public List<Key> Keys = new List<Key>();
        }

        public class Key
        {
            public float Time;
            public float X;
            public float Y;
            public float Z;
            public float W;
        }

        public static List<Anim> UnpackMOT(string path)
        {
            var anims = new List<Anim>();

            using (BinaryReaderExt r = new BinaryReaderExt(new FileStream(path, FileMode.Open)))
            {
                r.BigEndian = true;

                r.ReadInt32(); // unknown

                int count = r.ReadInt32();
                uint headerSize = r.ReadUInt32();
                uint fileLength = r.ReadUInt32();

                if (fileLength != r.Length)
                    throw new Exception("File Length Mismatch");

                for (uint i = 0; i < count; i++)
                {
                    r.Seek(headerSize + i * 4);
                    r.Seek(r.ReadUInt32());

                    Anim anim = new Anim();
                    if (r.Position != 0)
                        anim.Parse(r);
                    anims.Add(anim);
                }
            }
            return anims;
        }
    }
}
