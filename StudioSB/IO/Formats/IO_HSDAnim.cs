using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudioSB.Scenes;
using StudioSB.Scenes.Animation;
using HSDLib;
using HSDLib.Helpers;
using HSDLib.Animation;

namespace StudioSB.IO.Formats
{
    public class IO_HSDAnim : IImportableAnimation, IExportableAnimation
    {
        public string Name => "Dat Anim File";

        public string Extension => "aj.dat";

        public object Settings => null;

        public SBAnimation ImportSBAnimation(string FileName, SBSkeleton skeleton)
        {
            SBAnimation anim = new SBAnimation();

            HSDFile f = new HSDFile(FileName);

            foreach(var root in f.Roots)
            {
                if (root == null || root.Node == null)
                    continue;
                anim.Name = root.Name;
                if (root.Node is HSD_FigaTree tree)
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
                            if (att.Track == null)
                                continue;

                            SBTrackType trackType = SBTrackType.TranslateX;
                            switch ((JointTrackType)att.Track.AnimationType)
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
                            
                            FOBJFrameDecoder decoder = new FOBJFrameDecoder(att.Track);
                            var keys = decoder.GetKeys();

                            float prevCurve = 0;
                            for (int k = 0; k < keys.Count; k++)
                            {
                                var key = keys[k];

                                if (key.InterpolationType == HSDLib.Animation.InterpolationType.HermiteCurve)
                                {
                                    prevCurve = key.Tan;
                                    continue;
                                }

                                if(key.InterpolationType == HSDLib.Animation.InterpolationType.HermiteValue)
                                {
                                    if (k < keys.Count - 1 && keys[k + 1].InterpolationType == HSDLib.Animation.InterpolationType.HermiteCurve)
                                        track.AddKey(key.Frame, key.Value, Scenes.Animation.InterpolationType.Hermite, prevCurve, keys[k + 1].Tan);
                                    else
                                        track.AddKey(key.Frame, key.Value, Scenes.Animation.InterpolationType.Hermite, prevCurve);
                                }
                                else
                                {
                                    if (k < keys.Count-1 && keys[k + 1].InterpolationType == HSDLib.Animation.InterpolationType.HermiteCurve)
                                        track.AddKey(key.Frame, key.Value, hsdInterToInter[key.InterpolationType], key.Tan, keys[k + 1].Tan);
                                    else
                                        track.AddKey(key.Frame, key.Value, hsdInterToInter[key.InterpolationType], key.Tan);
                                }

                                if(key.InterpolationType == HSDLib.Animation.InterpolationType.Hermite)
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
            HSDFile file = new HSDFile();
            HSDRoot root = new HSDRoot();
            root.Name = animation.Name;
            //TODO: export option for name
            if (animation.Name == "")
                root.Name = System.IO.Path.GetFileNameWithoutExtension(FileName);
            file.Roots.Add(root);

            HSD_FigaTree tree = new HSD_FigaTree();
            tree.FrameCount = animation.FrameCount;
            tree.Type = 1;
            root.Node = tree;

            int boneIndex = -1;
            foreach(var skelnode in skeleton.Bones)
            {
                HSD_AnimNode animNode = new HSD_AnimNode();
                tree.Nodes.Add(animNode);

                boneIndex++;
                // skip trans n and rotn tracks
                //if (boneIndex == 0 || boneIndex == 1)
                //    continue;

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
                            InterpolationType = HSDLib.Animation.InterpolationType.Constant
                        });
                    }else
                    foreach (var key in track.Keys.Keys)
                    {
                        if (key.InTan != key.OutTan)
                            keys.Add(new FOBJKey()
                            {
                                Frame = key.Frame,
                                Tan = key.OutTan,
                                InterpolationType = HSDLib.Animation.InterpolationType.HermiteCurve
                            });
                        if (key.InterpolationType == Scenes.Animation.InterpolationType.Hermite &&
                            prevKey != null &&
                            prevKey.InterpolationType == key.InterpolationType &&
                            prevKey.InTan == key.InTan && prevKey.OutTan == key.OutTan)
                            keys.Add(new FOBJKey()
                            {
                                Frame = key.Frame,
                                Value = key.Value,
                                InterpolationType = HSDLib.Animation.InterpolationType.HermiteValue
                            });
                        else
                            keys.Add(new FOBJKey()
                            {
                                Frame = key.Frame,
                                Value = key.Value,
                                Tan = key.OutTan,
                                InterpolationType = ToGXInterpolation(key.InterpolationType)
                            });
                        prevKey = key;
                    }
                    animTrack.Track = FOBJFrameEncoder.EncodeFrames(keys, ToGXTrackType(track.Type));
                    animNode.Tracks.Add(animTrack);
                }
            }

            file.Save(FileName);
        }

        public Dictionary<HSDLib.Animation.InterpolationType, StudioSB.Scenes.Animation.InterpolationType> hsdInterToInter = new Dictionary<HSDLib.Animation.InterpolationType, StudioSB.Scenes.Animation.InterpolationType>()
        {
            { HSDLib.Animation.InterpolationType.Constant,  StudioSB.Scenes.Animation.InterpolationType.Constant},
            { HSDLib.Animation.InterpolationType.Hermite,  StudioSB.Scenes.Animation.InterpolationType.Hermite},
            { HSDLib.Animation.InterpolationType.Linear,  StudioSB.Scenes.Animation.InterpolationType.Linear},
            { HSDLib.Animation.InterpolationType.Step,  StudioSB.Scenes.Animation.InterpolationType.Step}
        };
        
        private HSDLib.Animation.InterpolationType ToGXInterpolation(StudioSB.Scenes.Animation.InterpolationType i)
        {
            switch (i)
            {
                case Scenes.Animation.InterpolationType.Constant:
                    return HSDLib.Animation.InterpolationType.Constant;
                case Scenes.Animation.InterpolationType.Hermite:
                    return HSDLib.Animation.InterpolationType.Hermite;
                case Scenes.Animation.InterpolationType.Linear:
                    return HSDLib.Animation.InterpolationType.Linear;
                case Scenes.Animation.InterpolationType.Step:
                    return HSDLib.Animation.InterpolationType.Step;
                default:
                    return HSDLib.Animation.InterpolationType.Constant;
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
