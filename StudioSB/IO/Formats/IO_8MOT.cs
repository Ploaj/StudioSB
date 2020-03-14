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
using System.Linq;

namespace StudioSB.IO.Formats
{
    public class IO_8MOT : IImportableAnimation, IExportableAnimation
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

        public static MOTINGSettings Settings = new MOTINGSettings();

        public string Name => "Eighting Motion Format";

        public string Extension => ".mota";

        object IExportableAnimation.Settings => Settings;

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
                Vector3 rot_axis = Vector3.Cross(dir, Vector3.UnitX).Normalized();
                final = Matrix4.CreateFromAxisAngle(rot_axis, rot_angle);
            }

            Matrix4 xrot;
            Matrix4.CreateRotationX(-angle * (float)Math.PI / 180, out xrot);

            final = Matrix4.CreateFromQuaternion(baseRotation.Inverted()) * (final * xrot);

            return final.ExtractRotation().Inverted();
        }

        public SBAnimation ImportSBAnimation(string FileName, SBSkeleton skeleton)
        {
            using (SBCustomDialog d = new SBCustomDialog(Settings))
            {
                d.ShowDialog();
            }

            return ImportSBAnimation(FileName, skeleton, Settings.JVCPath);
        }

        public SBAnimation ImportSBAnimation(string FileName, SBSkeleton skeleton, string jvcPath)
        {
            var sbAnim = new SBAnimation();

            var jointTable = GetJointTable(jvcPath);

            var anim = new Anim();
            using (BinaryReaderExt r = new BinaryReaderExt(new FileStream(FileName, FileMode.Open)))
                anim.Parse(r);

            float scale = Settings.FrameScale;
            sbAnim.FrameCount = (float)(scale * anim.EndTime);

            foreach (var j in anim.Joints)
            {
                if (j.BoneID == -1)
                    continue;

                if (j.BoneID >= jointTable.Length || jointTable[j.BoneID] == -1 || jointTable[j.BoneID] >= skeleton.Bones.Length)
                    continue;

                var bone = skeleton.Bones[jointTable[j.BoneID]];


                SBTransformAnimation node = sbAnim.TransformNodes.Find(e => e.Name == bone.Name);
                if (node == null)
                {
                    node = new SBTransformAnimation();
                    node.Name = bone.Name;
                    sbAnim.TransformNodes.Add(node);
                }
                //Console.WriteLine(bone.Name);

                if (j.Flag3 == 0x21)
                {
                    foreach (var k in j.Keys)
                    {
                        //Console.WriteLine($"\t{k.Time} {k.X} {k.Y} {k.Z} {k.W}");
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
        /// <param name="FileName"></param>
        /// <param name="animation"></param>
        /// <param name="skeleton"></param>
        public void ExportSBAnimation(string FileName, SBAnimation animation, SBSkeleton skeleton)
        {
            var jointTable = GetJointTable(Settings.JVCPath).ToList();

            Anim a = new Anim();

            a.EndTime = animation.FrameCount / Settings.FrameScale;
            a.PlaySpeed = 0.01f;

            a.Joints.Add(new Joint()
            {
                BoneID = -1,
                Flag3 = 0x40,
                MaxTime = a.EndTime,
            });

            List<double> AllKeys = new List<double>();

            foreach(var v in animation.TransformNodes)
            {
                if (skeleton[v.Name] == null)
                    continue;

                var sb = skeleton[v.Name];
                if (sb == null)
                    continue;

                var index = (short)jointTable.IndexOf((short)skeleton.IndexOfBone(sb));

                Console.WriteLine(v.Name + " " + index);

                if (index == -1)
                    continue;

                Joint Position = null;
                Joint Rotation = null;
                Joint Scale = null;

                if (v.HasTranslation)
                {
                    Position = new Joint();
                    var j = Position;
                    j.Flag1 = 0x02;
                    j.Flag2 = 0x02;
                    j.Flag3 = 0x21;
                    j.BoneID = index;
                    j.MaxTime = a.EndTime;
                }
                if (v.HasRotation)
                {
                    Rotation = new Joint();
                    var j = Rotation;
                    j.Flag1 = 0x02;
                    j.Flag2 = 0x02;
                    j.Flag3 = 0x28;
                    j.BoneID = index;
                    j.MaxTime = a.EndTime;
                }
                if (v.HasScale)
                {
                    Scale = new Joint();
                    var j = Scale;
                    j.Flag1 = 0x02;
                    j.Flag2 = 0x02;
                    j.Flag3 = 0x22;
                    j.BoneID = index;
                    j.MaxTime = a.EndTime;
                }

                // gather baked nodes
                List<Vector3> Positions = new List<Vector3>();
                List<Vector4> Rotations = new List<Vector4>();
                List<Vector3> Scales = new List<Vector3>();
                for (int i = 0; i <= animation.FrameCount; i++)
                {
                    var t = v.GetTransformAt(i, skeleton);

                    if (v.HasTranslation)
                        Positions.Add(t.ExtractTranslation() - sb.Translation);

                    if (v.HasScale)
                        Positions.Add(t.ExtractScale() - sb.Scale);

                    if (v.HasRotation)
                    {
                        var quat = (t.ExtractRotation().Inverted() * sb.RotationQuaternion).Inverted();

                        var inv = new Quaternion(quat.X, 0, 0, quat.W).Normalized().Inverted();

                        var dir = Vector3.TransformNormal(Vector3.UnitX, Matrix4.CreateFromQuaternion(quat * inv));
                        var angle = Tools.CrossMath.ToEulerAngles(inv).X * 180 / (float)Math.PI;

                        Rotations.Add(new Vector4(dir, angle));
                    }
                }

                // now we convert the bake nodes into linear tracks in the new time scale range
                if (Positions.Count > 0)
                {
                    var X = SimplifyLines(Positions.Select(e => e.X));
                    var Y = SimplifyLines(Positions.Select(e => e.Y));
                    var Z = SimplifyLines(Positions.Select(e => e.Z));

                    var frames = X.Select(e => e.Item1).Union(Y.Select(e => e.Item1).Union(Z.Select(e => e.Item1))).ToList();
                    frames.Sort();
                    if (frames[frames.Count - 1] != animation.FrameCount)
                        frames.Add(animation.FrameCount);

                    AllKeys = AllKeys.Union(frames).ToList();

                    foreach (var f in frames)
                    {
                        Position.Keys.Add(new Key()
                        {
                            Time = (float)f / Settings.FrameScale,
                            X = Positions[(int)f].X,
                            Y = Positions[(int)f].Y,
                            Z = Positions[(int)f].Z,
                            W = 0
                        });
                    }
                }
                if (Rotations.Count > 0)
                {
                    var X = SimplifyLines(Rotations.Select(e => e.X));
                    var Y = SimplifyLines(Rotations.Select(e => e.Y));
                    var Z = SimplifyLines(Rotations.Select(e => e.Z));
                    var W = SimplifyLines(Rotations.Select(e => e.W));

                    var frames = X.Select(e => e.Item1).Union(Y.Select(e => e.Item1).Union(Z.Select(e => e.Item1))).Union(W.Select(e => e.Item1)).ToList();
                    frames.Sort();
                    if (frames[frames.Count - 1] != animation.FrameCount)
                        frames.Add(animation.FrameCount);

                    AllKeys = AllKeys.Union(frames).ToList();

                    foreach (var f in frames)
                    {
                        Rotation.Keys.Add(new Key()
                        {
                            Time = (float)f / Settings.FrameScale,
                            X = Rotations[(int)f].X,
                            Y = Rotations[(int)f].Y,
                            Z = Rotations[(int)f].Z,
                            W = Rotations[(int)f].W,
                        });
                    }
                }

                if (Position != null)
                    a.Joints.Add(Position);
                if (Rotation != null)
                    a.Joints.Add(Rotation);
                if (Scale != null)
                    a.Joints.Add(Scale);
            }

            AllKeys.Sort();

            foreach(var v in AllKeys)
                a.Joints[0].Keys.Add(new Key() { Time = (float)v / Settings.FrameScale });
            
            a.Save(FileName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private List<Tuple<double, double>> SimplifyLines(IEnumerable<float> values)
        {
            List<Tuple<double, double>> keys = new List<Tuple<double, double>>();
            int i = 0;
            foreach (var val in values)
            {
                keys.Add(new Tuple<double, double>(i++, val));
            }
            List<Tuple<double, double>> newkeys = new List<Tuple<double, double>>();
            LineSimplification.RamerDouglasPeucker(keys, 0.1, newkeys);
            SBConsole.WriteLine("Simplified " + keys.Count + " to " + newkeys.Count);
            return newkeys;
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
        public class Anim
        {
            public float PlaySpeed;
            
            public float EndTime;
            
            public List<Joint> Joints = new List<Joint>();

            /// <summary>
            /// Saves to File
            /// </summary>
            /// <param name="filename"></param>
            public void Save(string filename)
            {
                using (Tools.BinaryWriterExt w = new Tools.BinaryWriterExt(new FileStream(filename, FileMode.Create)))
                {
                    w.BigEndian = true;
                    if (Joints.Count == 0)
                        return;
                    
                    var animStart = (uint)w.BaseStream.Position;
                    w.Write(Joints.Count);
                    w.Write(0x10);
                    w.Write(PlaySpeed);
                    w.Write(EndTime);

                    // padding
                    var headerStart = w.BaseStream.Position;
                    w.Write(new byte[Joints.Count * 0x20]);

                    for (int j = 0; j < Joints.Count; j++)
                    {
                        var joint = Joints[j];

                        var temp = (uint)w.BaseStream.Position;
                        w.Position = ((uint)(headerStart + j * 0x20));
                        w.Write(joint.Flag1);
                        w.Write(joint.Flag2);
                        w.Write(joint.Flag3);
                        w.Write(joint.BoneID);
                        w.Write((short)joint.Keys.Count);
                        w.Write(joint.MaxTime);
                        w.Write(joint.Unknown);
                        w.Position = temp;

                        w.WriteAt((int)(headerStart + j * 0x20 + 0x10), (int)(w.BaseStream.Position - animStart));
                        foreach (var k in joint.Keys)
                        {
                            w.Write(k.Time);
                        }
                        if (w.Position % 0x10 != 0)
                            w.Write(new byte[0x10 - w.Position % 0x10]);

                        if (joint.Flag1 == 0x02)
                        {
                            w.WriteAt((int)(headerStart + j * 0x20 + 0x14), (int)(w.BaseStream.Position - animStart));
                            foreach (var k in joint.Keys)
                            {
                                w.Write((BitConverter.ToInt16(BitConverter.GetBytes(k.X), 2)));
                                w.Write((BitConverter.ToInt16(BitConverter.GetBytes(k.Y), 2)));
                                w.Write((BitConverter.ToInt16(BitConverter.GetBytes(k.Z), 2)));
                                w.Write((BitConverter.ToInt16(BitConverter.GetBytes(k.W), 2)));
                            }
                            if (w.Position % 0x10 != 0)
                                w.Write(new byte[0x10 - w.Position % 0x10]);
                        }
                    }
                }
                    
            }

            public void Parse(BinaryReaderExt r)
            {
                r.BigEndian = true;

                var start = r.Position;

                var sectionCount = r.ReadInt32();
                var sectionHeaderLength = r.ReadInt32();
                PlaySpeed = r.ReadSingle();
                EndTime = r.ReadSingle();

                for (int j = 0; j < sectionCount; j++)
                {
                    Joint joint = new Joint();

                    Joints.Add(joint);

                    joint.Flag1 = r.ReadByte();
                    joint.Flag2 = r.ReadByte();
                    joint.Flag3 = r.ReadUInt16();
                    joint.BoneID = r.ReadInt16();
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
            
            public short BoneID;
        
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

        /// <summary>
        /// 
        /// </summary>
        public class Anims
        {
            public List<Anim> AnimList = new List<Anim>();
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
