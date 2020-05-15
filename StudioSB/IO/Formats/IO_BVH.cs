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
    public class IO_BVH : IExportableAnimation
    {
        public class BVHSettings
        {
            [DisplayName("Skeleton Only"),
                Description("Export the base skeleton with no animation")]
            public bool SkeletonOnly { get; set; } = false;
        }

        public static BVHSettings Settings = new BVHSettings();

        public string Name => "Biovision Hierarchy";

        public string Extension => ".bvh";

        object IExportableAnimation.Settings => Settings;

        private class Bone
        {
            public Bone parent;
            public SBBone head;
            public SBBone tail;
            public List<Bone> children;

            public String name
            {
                get
                {
                    return tail.Name;
                }
            }

            private Quaternion absoluteRotationQuaternion
            {
                get
                {
                    Vector3 headBasePos = Vector3.TransformPosition(Vector3.Zero, head.WorldTransform);
                    Vector3 tailBasePos = Vector3.TransformPosition(Vector3.Zero, tail.WorldTransform);
                    Vector3 dirBase = tailBasePos - headBasePos;
                    Vector3 headPos = Vector3.TransformPosition(Vector3.Zero, head.AnimatedWorldTransform);
                    Vector3 tailPos = Vector3.TransformPosition(Vector3.Zero, tail.AnimatedWorldTransform);
                    Vector3 dir = tailPos - headPos;
                    Quaternion quat = Tools.CrossMath.ToQuaternion(dirBase, dir);
                    return quat;
                }
            }

            public Vector3 rotation
            {
                get
                {
                    Quaternion quat = absoluteRotationQuaternion;

                    if (parent != null)
                    {
                        quat = parent.absoluteRotationQuaternion.Inverted() * quat;
                    }

                    return Tools.CrossMath.ToEulerAngles(quat.Inverted())
                           * 180 / (float)Math.PI;
                }
            }

            public Vector3 offset
            {
                get
                {
                    if (parent == null)
                    {
                        return Vector3.TransformPosition(Vector3.Zero, head.AnimatedWorldTransform);
                    }

                    Vector3 parentBasePos = Vector3.TransformPosition(Vector3.Zero, parent.head.WorldTransform);
                    Vector3 headBasePos = Vector3.TransformPosition(Vector3.Zero, head.WorldTransform);
                    Vector3 vecBase = headBasePos - parentBasePos;
                    Vector3 parentPos = Vector3.TransformPosition(Vector3.Zero, parent.head.AnimatedWorldTransform);
                    Vector3 headPos = Vector3.TransformPosition(Vector3.Zero, head.AnimatedWorldTransform);
                    Vector3 vec = headPos - parentPos;

                    if (vecBase.Length > 0)
                    {
                        float lengthFactor = vec.Length / vecBase.Length;
                        return vecBase * lengthFactor;
                    }
                    return vec;
                }
            }
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="animation"></param>
        /// <param name="skeleton"></param>
        public void ExportSBAnimation(string FileName, SBAnimation animation, SBSkeleton skeleton)
        {
            using (StreamWriter file = new StreamWriter(FileName))
                Write(file, animation, skeleton);
        }

        private List<Bone> CreateRoots(List<SBBone> sbBones)
        {
            List<Bone> roots = new List<Bone>();
            foreach (var head in sbBones)
            {
                foreach (var bone in CreateChildBones(head, null))
                {
                    roots.Add(bone);
                }
            }
            return roots;
        }

        private List<Bone> CreateChildBones(SBBone head, Bone parent)
        {
            List<Bone> bones = new List<Bone>();
            foreach (var tail in head.Children)
            {
                Bone bone = new Bone();
                bone.parent = parent;
                bone.head = head;
                bone.tail = tail;
                bone.children = CreateChildBones(tail, bone);
                bones.Add(bone);
            }
            return bones;
        }

        public void Write(StreamWriter file, SBAnimation animation, SBSkeleton skeleton)
        {
            List<Bone> roots = CreateRoots(skeleton.Roots.ToList());

            file.WriteLine("HIERARCHY");
            foreach (var root in roots)
            {
                Vector3 offset = Vector3.TransformPosition(Vector3.Zero, root.head.WorldTransform);
                file.WriteLine("ROOT " + root.name);
                file.WriteLine("{");
                file.WriteLine("\tOFFSET " + offset.X + " " + offset.Y + " " + offset.Z);
                file.WriteLine("\tCHANNELS 6 Xposition Yposition Zposition Zrotation Yrotation Xrotation");
                foreach (var child in root.children)
                {
                    WriteChildBone(file, child, "\t");
                }
                file.WriteLine("}");
            }

            int nFrames = !Settings.SkeletonOnly ? (int)animation.FrameCount : 1;

            file.WriteLine("MOTION");
            file.WriteLine("Frames: " + nFrames);
            file.WriteLine("Frame Time: .0083333");

            if (Settings.SkeletonOnly)
            {
                String line = "";
                for (int i = 0; i < roots.Count; i++)
                {
                    Bone root = roots[i];
                    Vector3 offset = Vector3.TransformPosition(Vector3.Zero, root.head.WorldTransform);
                    line += offset.X + " " + offset.Y + " " + offset.Z + " 0 0 0";
                    if (root.children.Count > 0)
                    {
                        line += " " + AnimateSkeletonOnly(root.children);
                    }
                    if (i < roots.Count - 1)
                    {
                        line += " ";
                    }
                }
                file.WriteLine(line);
                return;
            }

            Dictionary<String, SBTransformAnimation> transformNodes = new Dictionary<string, SBTransformAnimation>();
            foreach (var v in animation.TransformNodes)
            {
                transformNodes[v.Name] = v;
            }

            for (int frame = 0; frame < nFrames; frame++)
            {
                String line = "";
                UpdateSBSkeleton(skeleton, transformNodes, frame);
                for (int i = 0; i < roots.Count; i++)
                {
                    Bone root = roots[i];
                    Vector3 pos = root.offset;
                    line += pos.X + " " + pos.Y + " " + pos.Z + " 0 0 0";

                    foreach (var child in root.children)
                    {
                        line += " " + Animate(child);
                    }
                    if (i < roots.Count - 1)
                    {
                        line += " ";
                    }
                }
                file.WriteLine(line);
            }
        }

        private void UpdateSBSkeleton(SBSkeleton skeleton, Dictionary<String, SBTransformAnimation> transformNodes, int frame)
        {
            foreach (var nodeName in transformNodes.Keys)
            {
                SBTransformAnimation a = transformNodes[nodeName];
                SBBone bone = skeleton[nodeName];
                bone.AnimatedTransform = a.GetTransformAt(frame, bone);
            }
        }

        private void WriteChildBone(StreamWriter file, Bone bone, String tabs)
        {
            Vector3 offset = Vector3.TransformPosition(Vector3.Zero, bone.head.WorldTransform) -
                             Vector3.TransformPosition(Vector3.Zero, bone.head.Parent.WorldTransform);
            file.WriteLine(tabs + "JOINT " + bone.name);
            file.WriteLine(tabs + "{");
            file.WriteLine(tabs + "\tOFFSET " + offset.X + " " + offset.Y + " " + offset.Z);
            file.WriteLine(tabs + "\tCHANNELS 3 Xposition Yposition Zposition Zrotation Yrotation Xrotation");
            if (bone.children.Count > 0)
            {
                foreach (var child in bone.children)
                {
                    WriteChildBone(file, child, tabs + "\t");
                }
            } else
            {
                offset = Vector3.TransformPosition(Vector3.Zero, bone.tail.WorldTransform) -
                         Vector3.TransformPosition(Vector3.Zero, bone.head.WorldTransform);
                file.WriteLine(tabs + "\tEnd Site");
                file.WriteLine(tabs + "\t{");
                file.WriteLine(tabs + "\t\tOFFSET " + offset.X + " " + offset.Y + " " + offset.Z);
                file.WriteLine(tabs + "\t}");
            }
            file.WriteLine(tabs + "}");
        }

        private String Animate(Bone bone)
        {
            String line = "";

            Vector3 rot = bone.rotation;
            Vector3 offset = bone.offset;
            line += offset.X + " " + offset.Y + " " + offset.Z + " " + rot.Z + " " + rot.Y + " " + rot.X;

            for (int i = 0; i < bone.children.Count; i++)
            {
                Bone child = bone.children[i];
                line += " " + Animate(child);
            }

            return line;
        }

        private String AnimateSkeletonOnly(List<Bone> bones)
        {
            String line = "";
            for (int i = 0; i < bones.Count; i++)
            {
                Bone bone = bones[i];
                Vector3 offset = bone.offset;
                line += offset.X + " " + offset.Y + " " + offset.Z + " 0 0 0";
                if (bone.children.Count > 0)
                {
                    line += " " + AnimateSkeletonOnly(bone.children);
                }
                if (i < bones.Count - 1)
                {
                    line += " ";
                }
            }
            return line;
        }
    }
}