using System;
using System.Collections.Generic;
using StudioSB.Scenes;
using StudioSB.Scenes.Animation;
using HSDRaw;
using HSDRaw.Tools;
using HSDRaw.Common.Animation;

namespace StudioSB.IO.Formats
{
    public class IO_HSDAnim : IImportableAnimation, IExportableAnimation
    {
        public class HSDAnimSettings
        {
            public string RootName { get; set; }
        }

        public string Name => "Dat Anim File";

        public string Extension => ".dat";

        public static HSDAnimSettings HSDSettings = new HSDAnimSettings();

        public object Settings => HSDSettings;

        public SBAnimation ImportSBAnimation(string FileName, SBSkeleton skeleton)
        {
            SBAnimation anim = new SBAnimation();

            HSDRawFile f = new HSDRawFile(FileName);

            foreach(var root in f.Roots)
            {
                if (root == null || root.Data == null)
                    continue;
                anim.Name = root.Name;
                if (root.Data is HSD_FigaTree tree)
                {
                    anim.FrameCount = tree.FrameCount;
                    int nodeIndex = 0;
                    foreach(var node in tree.Nodes)
                    {
                        SBTransformAnimation a = new SBTransformAnimation();
                        a.Name = "JOBJ_" + nodeIndex++;
                        anim.TransformNodes.Add(a);

                        foreach (var att in node.Tracks)
                        {
                            if (att.FOBJ == null)
                                continue;

                            SBTrackType trackType = SBTrackType.TranslateX;
                            switch (att.FOBJ.AnimationType)
                            {
                                case JointTrackType.HSD_A_J_ROTX: trackType = SBTrackType.RotateX; break;
                                case JointTrackType.HSD_A_J_ROTY: trackType = SBTrackType.RotateY; break;
                                case JointTrackType.HSD_A_J_ROTZ: trackType = SBTrackType.RotateZ; break;
                                case JointTrackType.HSD_A_J_TRAX: trackType = SBTrackType.TranslateX; break;
                                case JointTrackType.HSD_A_J_TRAY: trackType = SBTrackType.TranslateY; break;
                                case JointTrackType.HSD_A_J_TRAZ: trackType = SBTrackType.TranslateZ; break;
                                case JointTrackType.HSD_A_J_SCAX: trackType = SBTrackType.ScaleX; break;
                                case JointTrackType.HSD_A_J_SCAY: trackType = SBTrackType.ScaleY; break;
                                case JointTrackType.HSD_A_J_SCAZ: trackType = SBTrackType.ScaleZ; break;
                            }

                            SBTransformTrack track = new SBTransformTrack(trackType);
                            a.Tracks.Add(track);
                            
                            FOBJFrameDecoder decoder = new FOBJFrameDecoder(att.FOBJ);
                            var keys = decoder.GetKeys();

                            float prevCurve = 0;
                            for (int k = 0; k < keys.Count; k++)
                            {
                                var key = keys[k];

                                if (key.InterpolationType == GXInterpolationType.HermiteCurve)
                                {
                                    prevCurve = key.Tan;
                                    continue;
                                }

                                if(key.InterpolationType == GXInterpolationType.HermiteValue)
                                {
                                    if (k < keys.Count - 1 && keys[k + 1].InterpolationType == GXInterpolationType.HermiteCurve)
                                        track.AddKey(key.Frame, key.Value, InterpolationType.Hermite, prevCurve, keys[k + 1].Tan);
                                    else
                                        track.AddKey(key.Frame, key.Value, InterpolationType.Hermite, prevCurve);
                                }
                                else
                                {
                                    if (k < keys.Count-1 && keys[k + 1].InterpolationType == GXInterpolationType.HermiteCurve)
                                        track.AddKey(key.Frame, key.Value, hsdInterToInter[key.InterpolationType], key.Tan, keys[k + 1].Tan);
                                    else
                                        track.AddKey(key.Frame, key.Value, hsdInterToInter[key.InterpolationType], key.Tan);
                                }

                                if(key.InterpolationType == GXInterpolationType.Hermite)
                                prevCurve = key.Tan;
                            }
                        }
                    }
                }
            }

            return anim;
        }

        public void ExportSBAnimation(string FileName, SBAnimation animation, SBSkeleton skeleton)
        {
            HSDRawFile file = new HSDRawFile();
            HSDRootNode root = new HSDRootNode();
            
            if (HSDSettings.RootName == "" || HSDSettings.RootName == null)
                HSDSettings.RootName = System.IO.Path.GetFileNameWithoutExtension(FileName);

            if (HSDSettings.RootName == "" || HSDSettings.RootName == null)
                HSDSettings.RootName = animation.Name;
            
            root.Name = HSDSettings.RootName;

            if (root.Name == null || !root.Name.EndsWith("_figatree"))
            {
                System.Windows.Forms.MessageBox.Show($"Warning, the root name does not end with \"_figatree\"\n{root.Name}");
            }

            file.Roots.Add(root);

            var nodes = new List<FigaTreeNode>();

            int boneIndex = -1;
            foreach(var skelnode in skeleton.Bones)
            {
                FigaTreeNode animNode = new FigaTreeNode();
                nodes.Add(animNode);

                boneIndex++;
                // skip trans n and rotn tracks
                if (boneIndex == 0)
                    continue;

                var node = animation.TransformNodes.Find(e => e.Name == skelnode.Name);
                if (node == null)
                    continue;

                foreach (var track in node.Tracks)
                {
                    HSD_Track animTrack = new HSD_Track();
                    List<FOBJKey> keys = new List<FOBJKey>();
                    SBAnimKey<float> prevKey = null;

                    var constant = true;
                    var v = track.Keys.Keys.Count > 0 ? track.Keys.Keys[0].Value : 0;
                    foreach (var key in track.Keys.Keys)
                    {
                        if (key.Value != v)
                        {
                            constant = false;
                            break;
                        }
                    }
                    if (constant)
                    {
                        keys.Add(new FOBJKey()
                        {
                            Frame = 0,
                            Value = v,
                            InterpolationType = GXInterpolationType.Constant
                        });
                    }
                    else
                    for(int i = 0; i < track.Keys.Keys.Count; i++)
                        {
                            var key = track.Keys.Keys[i];
                            if (i > 0 && track.Keys.Keys[i-1].InTan != track.Keys.Keys[i - 1].OutTan)
                                keys.Add(new FOBJKey()
                                {
                                    Frame = key.Frame,
                                    Tan = track.Keys.Keys[i - 1].OutTan,
                                    InterpolationType = GXInterpolationType.HermiteCurve
                                });

                            if (key.InterpolationType == Scenes.Animation.InterpolationType.Hermite &&
                                prevKey != null &&
                                prevKey.InterpolationType == key.InterpolationType &&
                                prevKey.InTan == key.InTan && prevKey.OutTan == key.OutTan)
                                keys.Add(new FOBJKey()
                                {
                                    Frame = key.Frame,
                                    Value = key.Value,
                                    InterpolationType = GXInterpolationType.HermiteValue
                                });
                            else
                                keys.Add(new FOBJKey()
                                {
                                    Frame = key.Frame,
                                    Value = key.Value,
                                    Tan = key.InTan,
                                    InterpolationType = ToGXInterpolation(key.InterpolationType)
                                });
                            prevKey = key;
                        }
                    animTrack.FOBJ = FOBJFrameEncoder.EncodeFrames(keys, ToGXTrackType(track.Type));
                    animTrack.DataLength = (short)animTrack.FOBJ.Buffer.Length;
                    animNode.Tracks.Add(animTrack);
                }
            }

            HSD_FigaTree tree = new HSD_FigaTree();
            tree.FrameCount = animation.FrameCount;
            tree.Type = 1;
            tree.Nodes = nodes;

            SBConsole.WriteLine(tree.FrameCount);

            root.Data = tree;
            file.Save(FileName);
        }

        public Dictionary<GXInterpolationType, InterpolationType> hsdInterToInter = new Dictionary<GXInterpolationType, InterpolationType>()
        {
            { GXInterpolationType.Constant,  InterpolationType.Constant},
            { GXInterpolationType.Hermite,  InterpolationType.Hermite},
            { GXInterpolationType.Linear,  InterpolationType.Linear},
            { GXInterpolationType.Step,  InterpolationType.Step}
        };
        
        private GXInterpolationType ToGXInterpolation(InterpolationType i)
        {
            switch (i)
            {
                case InterpolationType.Constant:
                    return GXInterpolationType.Constant;
                case InterpolationType.Hermite:
                    return GXInterpolationType.Hermite;
                case InterpolationType.Linear:
                    return GXInterpolationType.Linear;
                case InterpolationType.Step:
                    return GXInterpolationType.Step;
                default:
                    return GXInterpolationType.Constant;
            }
        }

        public static byte ToGXTrackType(SBTrackType type)
        {
            switch (type)
            {
                case SBTrackType.TranslateX:
                    return (byte)JointTrackType.HSD_A_J_TRAX;
                case SBTrackType.TranslateY:
                    return (byte)JointTrackType.HSD_A_J_TRAY;
                case SBTrackType.TranslateZ:
                    return (byte)JointTrackType.HSD_A_J_TRAZ;
                case SBTrackType.RotateX:
                    return (byte)JointTrackType.HSD_A_J_ROTX;
                case SBTrackType.RotateY:
                    return (byte)JointTrackType.HSD_A_J_ROTY;
                case SBTrackType.RotateZ:
                    return (byte)JointTrackType.HSD_A_J_ROTZ;
                case SBTrackType.ScaleX:
                    return (byte)JointTrackType.HSD_A_J_SCAX;
                case SBTrackType.ScaleY:
                    return (byte)JointTrackType.HSD_A_J_SCAY;
                case SBTrackType.ScaleZ:
                    return (byte)JointTrackType.HSD_A_J_SCAZ;
                default:
                    return 0;
            }
        }
    }
}
