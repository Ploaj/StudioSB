using StudioSB.Scenes;
using StudioSB.Scenes.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using System.ComponentModel;

namespace StudioSB.IO.Formats
{
    public class IO_MayaANIM : IExportableAnimation
    {
        public class ExportSettings
        {
            [DisplayName("Use Maya <= 2015"), Description("")]
            public bool Maya2015 { get; set; } = false;

            [DisplayName("Use Radians"), Description("")]
            public bool UseRadians { get; set; } = true;
        }

        public string Name => "Maya ANIM";

        public string Extension => ".anim";

        object IExportableAnimation.Settings { get { return MayaSettings; } }

        private static ExportSettings MayaSettings = new ExportSettings();

        private enum InfinityType
        {
            constant,
            linear,
            cycle,
            cycleRelative,
            oscillate
        }

        private enum InputType
        {
            time,
            unitless
        }

        private enum OutputType
        {
            time,
            linear,
            angular,
            unitless
        }

        private enum ControlType
        {
            translate,
            rotate,
            scale
        }

        private enum TrackType
        {
            translateX,
            translateY,
            translateZ,
            rotateX,
            rotateY,
            rotateZ,
            scaleX,
            scaleY,
            scaleZ
        }

        private class Header
        {
            public float animVersion;
            public string mayaVersion;
            public float startTime;
            public float endTime;
            public float startUnitless;
            public float endUnitless;
            public string timeUnit;
            public string linearUnit;
            public string angularUnit;

            public Header()
            {
                animVersion = 1.1f;
                mayaVersion = "2015";
                startTime = 1;
                endTime = 1;
                startUnitless = 0;
                endUnitless = 0;
                timeUnit = "ntscf";
                linearUnit = "cm";
                angularUnit = "rad";
            }
        }

        private class AnimKey
        {
            public float input, output;
            public string intan, outtan;
            public float t1 = 0, w1 = 1;

            public AnimKey()
            {
                intan = "linear";
                outtan = "linear";
            }
        }

        private class AnimData
        {
            public ControlType controlType;
            public TrackType type;
            public InputType input;
            public OutputType output;
            public InfinityType preInfinity, postInfinity;
            public bool weighted = false;
            public List<AnimKey> keys = new List<AnimKey>();

            public AnimData()
            {
                input = InputType.time;
                output = OutputType.linear;
                preInfinity = InfinityType.constant;
                postInfinity = InfinityType.constant;
                weighted = false;
            }
        }

        private class AnimBone
        {
            public string name;
            public List<AnimData> atts = new List<AnimData>();
        }

        private Header header;
        private List<AnimBone> Bones = new List<AnimBone>();

        public IO_MayaANIM()
        {
            header = new IO_MayaANIM.Header();
        }

        public void Save(string fileName)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName))
            {
                file.WriteLine("animVersion " + header.animVersion + ";");
                file.WriteLine("mayaVersion " + header.mayaVersion + ";");
                file.WriteLine("timeUnit " + header.timeUnit + ";");
                file.WriteLine("linearUnit " + header.linearUnit + ";");
                file.WriteLine("angularUnit " + header.angularUnit + ";");
                file.WriteLine("startTime " + 1 + ";");
                file.WriteLine("endTime " + header.endTime + ";");

                int Row = 0;

                foreach (AnimBone animBone in Bones)
                {
                    int TrackIndex = 0;
                    if (animBone.atts.Count == 0)
                    {
                        file.WriteLine($"anim {animBone.name} 0 1 {TrackIndex++};");
                    }
                    foreach (AnimData animData in animBone.atts)
                    {
                        file.WriteLine($"anim {animData.controlType}.{animData.type} {animData.type} {animBone.name} 0 1 {TrackIndex++};");
                        file.WriteLine("animData {");
                        file.WriteLine($" input {animData.input};");
                        file.WriteLine($" output {animData.output};");
                        file.WriteLine($" weighted {(animData.weighted ? 1 : 0)};");
                        file.WriteLine($" preInfinity {animData.preInfinity};");
                        file.WriteLine($" postInfinity {animData.postInfinity};");

                        file.WriteLine(" keys {");
                        foreach (AnimKey key in animData.keys)
                        {
                            // TODO: fixed splines
                            file.WriteLine($" {key.input} {key.output:N6} {key.intan} {key.outtan} 1 1 0;");
                        }
                        file.WriteLine(" }");

                        file.WriteLine("}");
                    }
                    Row++;
                }
            }
        }

        public static List<SBBone> getBoneTreeOrder(SBSkeleton Skeleton)
        {
            if (Skeleton.Bones.Length == 0)
                return null;
            List<SBBone> bone = new List<SBBone>();
            Queue<SBBone> q = new Queue<SBBone>();

            foreach (SBBone b in Skeleton.Bones)
            {
                QueueBones(b, q, Skeleton);
            }

            while (q.Count > 0)
            {
                bone.Add(q.Dequeue());
            }
            return bone;
        }

        public static void QueueBones(SBBone b, Queue<SBBone> q, SBSkeleton Skeleton)
        {
            q.Enqueue(b);
            foreach (SBBone c in b.Children)
                QueueBones(c, q, Skeleton);
        }

        public static void ExportIOAnimationAsANIM(string fname, SBAnimation animation, SBSkeleton Skeleton)
        {
            IO_MayaANIM anim = new IO_MayaANIM();

            anim.header.endTime = animation.FrameCount + 1;
            if (!MayaSettings.UseRadians)
                anim.header.angularUnit = "deg";

            // get bone order
            List<SBBone> BonesInOrder = getBoneTreeOrder(Skeleton);
            
            if (MayaSettings.Maya2015)
            {
                BonesInOrder = BonesInOrder.OrderBy(f => f.Name, StringComparer.Ordinal).ToList();
            }

            foreach (SBBone b in BonesInOrder)
            {
                AnimBone animBone = new AnimBone()
                {
                    name = b.Name
                };
                anim.Bones.Add(animBone);
                // Add Tracks
                SBTransformAnimation node = null;
                foreach (var animNode in animation.TransformNodes)
                {
                    if (animNode.Name.Equals(b.Name))
                    {
                        node = animNode;
                        break;
                    }
                }
                if (node == null) continue;

                //TODO: bake scale for compensate scale...

                AddAnimData(animBone, node.Transform, ControlType.translate, TrackType.translateX);
                AddAnimData(animBone, node.Transform, ControlType.translate, TrackType.translateY);
                AddAnimData(animBone, node.Transform, ControlType.translate, TrackType.translateZ);
                
                AddAnimData(animBone, node.Transform, ControlType.rotate, TrackType.rotateX);
                AddAnimData(animBone, node.Transform, ControlType.rotate, TrackType.rotateY);
                AddAnimData(animBone, node.Transform, ControlType.rotate, TrackType.rotateZ);
                
                AddAnimData(animBone, node.Transform, ControlType.scale, TrackType.scaleX);
                AddAnimData(animBone, node.Transform, ControlType.scale, TrackType.scaleY);
                AddAnimData(animBone, node.Transform, ControlType.scale, TrackType.scaleZ);
            }
            

            anim.Save(fname);
        }

        private static void AddAnimData(AnimBone animBone, SBKeyGroup<Matrix4> node, ControlType ctype, TrackType ttype)
        {
            AnimData d = new AnimData();
            d.controlType = ctype;
            d.type = ttype;
            foreach (var key in node.Keys)
            {
                AnimKey animKey = new AnimKey()
                {
                    input = key.Frame + 1,
                    output = GetValue(key.Value, ctype, ttype)
                };
                d.keys.Add(animKey);
            }

            if (d.keys.Count > 0)
                animBone.atts.Add(d);
        }

        private static float GetValue(Matrix4 transform, ControlType ctype, TrackType ttype)
        {
            SBBone temp = new SBBone();
            temp.Transform = transform;
            float rotationTransform = (float)(MayaSettings.UseRadians ? 1 : (180 / Math.PI));
            switch (ctype)
            {
                case ControlType.rotate:
                    return GetTrackValue(temp.RotationEuler, ttype) * rotationTransform;
                case ControlType.scale:
                    return GetTrackValue(temp.Scale, ttype);
                case ControlType.translate:
                    return GetTrackValue(temp.Translation, ttype);
            }
            return 0;
        }

        private static float GetTrackValue(Vector3 vector, TrackType ttype)
        {
            if (ttype.ToString().EndsWith("X"))
                return vector.X;
            if (ttype.ToString().EndsWith("Y"))
                return vector.Y;
            if (ttype.ToString().EndsWith("Z"))
                return vector.Z;

            return 0;
        }

        public void ExportSBAnimation(string FileName, SBAnimation animation, SBSkeleton skeleton)
        {
            ExportIOAnimationAsANIM(FileName, animation, skeleton);
        }
    }
}
