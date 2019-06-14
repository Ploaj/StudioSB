using StudioSB.Scenes;
using StudioSB.Scenes.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using System.ComponentModel;
using System.IO;

namespace StudioSB.IO.Formats
{
    public class IO_MayaANIM : IExportableAnimation, IImportableAnimation
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
            scale,
            visibility
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
            scaleZ,
            visibility
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
            public float t2 = 0, w2 = 1;

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
            header = new Header();
        }

        public void Open(string fileName)
        {
            using (StreamReader r = new StreamReader(new FileStream(fileName, FileMode.Open)))
            {
                AnimData currentData = null;
                while (!r.EndOfStream)
                {
                    var line = r.ReadLine();
                    var args = line.Trim().Replace(";", "").Split(' ');

                    switch (args[0])
                    {
                        case "animVersion":
                            header.animVersion = float.Parse(args[1]);
                            break;
                        case "mayaVersion":
                            header.mayaVersion = args[1];
                            break;
                        case "timeUnit":
                            header.timeUnit = args[1];
                            break;
                        case "linearUnit":
                            header.linearUnit = args[1];
                            break;
                        case "angularUnit":
                            header.angularUnit = args[1];
                            break;
                        case "startTime":
                            header.startTime = float.Parse(args[1]);
                            break;
                        case "endTime":
                            header.endTime = float.Parse(args[1]);
                            break;
                        case "anim":
                            if (args.Length != 7)
                                continue;
                            var currentNode = Bones.Find(e => e.name.Equals(args[3]));
                            if(currentNode == null)
                            {
                                currentNode = new AnimBone();
                                currentNode.name = args[3];
                                Bones.Add(currentNode);
                            }
                            currentData = new AnimData();
                            currentData.controlType = (ControlType)Enum.Parse(typeof(ControlType), args[1].Split('.')[0]);
                            currentData.type = (TrackType)Enum.Parse(typeof(TrackType), args[2]);
                            currentNode.atts.Add(currentData);
                            break;
                        case "animData":
                            if (currentData == null)
                                continue;
                            string dataLine = r.ReadLine();
                            while (!dataLine.Contains("}"))
                            {
                                var dataArgs = dataLine.Trim().Replace(";", "").Split(' ');
                                switch (dataArgs[0])
                                {
                                    case "input":
                                        currentData.input = (InputType)Enum.Parse(typeof(InputType), dataArgs[1]);
                                        break;
                                    case "output":
                                        currentData.output = (OutputType)Enum.Parse(typeof(OutputType), dataArgs[1]);
                                        break;
                                    case "weighted":
                                        currentData.weighted = dataArgs[1] == "1";
                                        break;
                                    case "preInfinity":
                                        currentData.preInfinity = (InfinityType)Enum.Parse(typeof(InfinityType), dataArgs[1]);
                                        break;
                                    case "postInfinity":
                                        currentData.postInfinity = (InfinityType)Enum.Parse(typeof(InfinityType), dataArgs[1]);
                                        break;
                                    case "keys":
                                        string keyLine = r.ReadLine();
                                        while (!keyLine.Contains("}"))
                                        {
                                            var keyArgs = keyLine.Trim().Replace(";", "").Split(' ');

                                            var key = new AnimKey()
                                            {
                                                input = float.Parse(keyArgs[0]),
                                                output = float.Parse(keyArgs[1])
                                            };

                                            if(keyArgs.Length >= 7)
                                            {
                                                key.intan = keyArgs[2];
                                                key.outtan = keyArgs[3];
                                            }

                                            if(key.intan == "fixed")
                                            {
                                                key.t1 = float.Parse(keyArgs[7]);
                                                key.w1 = float.Parse(keyArgs[8]);
                                            }
                                            if (key.outtan == "fixed")
                                            {
                                                key.t2 = float.Parse(keyArgs[9]);
                                                key.w2 = float.Parse(keyArgs[10]);
                                            }

                                            currentData.keys.Add(key);

                                            keyLine = r.ReadLine();
                                        }
                                        break;

                                }
                                dataLine = r.ReadLine();
                            }
                            break;
                    }
                }
            }
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
                            string tanin = key.intan == "fixed" ? " " + key.t1 + " " + key.w1 : "";
                            string tanout = key.outtan == "fixed" ? " " + key.t2 + " " + key.w2 : "";
                            file.WriteLine($" {key.input} {key.output:N6} {key.intan} {key.outtan} 1 1 0{tanin}{tanout};");
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

            foreach (SBBone b in Skeleton.Roots)
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
                
                foreach (var track in node.Tracks)
                {
                    switch (track.Type)
                    {
                        case SBTrackType.TranslateX:
                            AddAnimData(animBone, track.Keys, ControlType.translate, TrackType.translateX);
                            break;
                        case SBTrackType.TranslateY:
                            AddAnimData(animBone, track.Keys, ControlType.translate, TrackType.translateY);
                            break;
                        case SBTrackType.TranslateZ:
                            AddAnimData(animBone, track.Keys, ControlType.translate, TrackType.translateZ);
                            break;
                        case SBTrackType.RotateX:
                            AddAnimData(animBone, track.Keys, ControlType.rotate, TrackType.rotateX);
                            break;
                        case SBTrackType.RotateY:
                            AddAnimData(animBone, track.Keys, ControlType.rotate, TrackType.rotateY);
                            break;
                        case SBTrackType.RotateZ:
                            AddAnimData(animBone, track.Keys, ControlType.rotate, TrackType.rotateZ);
                            break;
                        case SBTrackType.ScaleX:
                            AddAnimData(animBone, track.Keys, ControlType.scale, TrackType.scaleX);
                            break;
                        case SBTrackType.ScaleY:
                            AddAnimData(animBone, track.Keys, ControlType.scale, TrackType.scaleY);
                            break;
                        case SBTrackType.ScaleZ:
                            AddAnimData(animBone, track.Keys, ControlType.scale, TrackType.scaleZ);
                            break;
                    }
                }
                
            }
            

            anim.Save(fname);
        }

        private static void AddAnimData(AnimBone animBone, SBKeyGroup<float> keys, ControlType ctype, TrackType ttype)
        {
            AnimData d = new AnimData();
            d.controlType = ctype;
            d.type = ttype;
            if(IsAngular(ctype))
                d.output = OutputType.angular;

            float value = 0;
            if (keys.Keys.Count > 0)
                value = keys.Keys[0].Value;

            bool IsConstant = true;
            foreach (var key in keys.Keys)
            {
                if(key.Value != value)
                {
                    IsConstant = false;
                    break;
                }
            }
            foreach (var key in keys.Keys)
            {
                AnimKey animKey = new AnimKey()
                {
                    input = key.Frame + 1,
                    output = IsAngular(ctype) ? (MayaSettings.UseRadians ? key.Value : (float)(key.Value * (180/Math.PI))) : key.Value,
                };
                if (key.InterpolationType == InterpolationType.Hermite)
                {
                    animKey.intan = "fixed";
                    animKey.outtan = "fixed";
                    animKey.t1 = key.InTan;
                    animKey.t2 = key.OutTan;
                }
                d.keys.Add(animKey);
                if (IsConstant)
                    break;
            }

            if (d.keys.Count > 0)
                animBone.atts.Add(d);
        }

        private static bool IsAngular(ControlType type)
        {
            if (type == ControlType.rotate)
                return true;
            return false;
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

        private static Dictionary<TrackType, SBTrackType> trackTypeConversion = new Dictionary<TrackType, SBTrackType>()
        {
            { TrackType.translateX, SBTrackType.TranslateX },
            { TrackType.translateY, SBTrackType.TranslateY },
            { TrackType.translateZ, SBTrackType.TranslateZ },
            { TrackType.rotateX, SBTrackType.RotateX },
            { TrackType.rotateY, SBTrackType.RotateY },
            { TrackType.rotateZ, SBTrackType.RotateZ },
            { TrackType.scaleX, SBTrackType.ScaleX },
            { TrackType.scaleY, SBTrackType.ScaleY },
            { TrackType.scaleZ, SBTrackType.ScaleZ },
        };

        public SBAnimation ImportSBAnimation(string FileName, SBSkeleton skeleton)
        {
            IO_MayaANIM anim = new IO_MayaANIM();
            anim.Open(FileName);

            var animation = new SBAnimation();
            animation.Name = Path.GetFileNameWithoutExtension(FileName);
            animation.FrameCount = anim.header.endTime - 1;
            
            foreach(var node in anim.Bones)
            {
                SBTransformAnimation a = new SBTransformAnimation();
                a.Name = node.name;
                animation.TransformNodes.Add(a);
                
                foreach (var att in node.atts)
                {
                    if (!trackTypeConversion.ContainsKey(att.type))
                        continue;

                    SBTransformTrack track = new SBTransformTrack(trackTypeConversion[att.type]);
                    a.Tracks.Add(track);

                    foreach(var key in att.keys)
                    {
                        InterpolationType itype = InterpolationType.Linear;
                        float intan = 0;
                        float outtan = 0;
                        if (key.intan == "fixed")
                        {
                            itype = InterpolationType.Hermite;
                            intan = key.t1;
                        }
                        if (key.outtan == "fixed")
                        {
                            itype = InterpolationType.Hermite;
                            outtan = key.t2;
                        }
                        track.AddKey(key.input - header.startTime, 
                            GetOutputValue(anim, att, key.output), 
                            itype, 
                            intan
                            , outtan);
                    }
                }
            }

            return animation;
        }

        private float GetOutputValue(IO_MayaANIM anim, AnimData data, float value)
        {
            if(data.output == OutputType.angular)
            {
                if (anim.header.angularUnit == "deg")
                    return (float)(value * Math.PI / 180);
            }
            return value;
        }
    }
}
