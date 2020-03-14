using System;
using System.Collections.Generic;
using StudioSB.Scenes;
using StudioSB.Scenes.Animation;
using HSDRaw;
using HSDRaw.Tools;
using HSDRaw.Common.Animation;
using System.ComponentModel;
using HSDRaw.Common;
using OpenTK.Graphics.OpenGL;

namespace StudioSB.IO.Formats
{
    public class IO_HSDAnim : IImportableAnimation, IExportableAnimation
    {
        public class HSDAnimSettings
        {
            public string RootName { get; set; }

            [Description("Used for fighter animations, set to false for stages")]
            public bool FigaTree { get; set; } = true;

            [Category("Anim Joint Only"), DisplayName("Loop Animation"), Description("Determines whether the animation should loop once completing")]
            public bool LoopAnimation { get; set; } = false;
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
                if (root.Data is HSD_AnimJoint joint)
                {
                    var joints = joint.BreathFirstList;

                    int nodeIndex = -1;
                    foreach(var j in joints)
                    {
                        nodeIndex++;
                        if (j.AOBJ == null || j.AOBJ.FObjDesc == null)
                            continue;

                        SBConsole.WriteLine(nodeIndex + " " + j.Flags.ToString("X8") + " " + j.AOBJ.Flags.ToString());

                        SBTransformAnimation a = new SBTransformAnimation();
                        if (nodeIndex < skeleton.Bones.Length)
                            a.Name = skeleton.Bones[nodeIndex].Name;
                        else
                            a.Name = "JOBJ_" + nodeIndex;
                        anim.TransformNodes.Add(a);

                        anim.FrameCount = Math.Max(anim.FrameCount, j.AOBJ.EndFrame);

                        foreach (var fobj in j.AOBJ.FObjDesc.List)
                        {
                            a.Tracks.Add(DecodeFOBJ(fobj.ToFOBJ()));
                        }
                    }
                }
                if (root.Data is HSD_FigaTree tree)
                {
                    anim.FrameCount = tree.FrameCount;
                    int nodeIndex = 0;
                    foreach(var node in tree.Nodes)
                    {
                        SBTransformAnimation a = new SBTransformAnimation();
                        a.Name = skeleton.Bones[nodeIndex++].Name;
                        anim.TransformNodes.Add(a);

                        foreach (var att in node.Tracks)
                        {
                            if (att.FOBJ == null)
                                continue;
                            
                            a.Tracks.Add(DecodeFOBJ(att.FOBJ));
                        }
                    }
                }
                if (root.Data is HSD_MatAnimJoint matjoint)
                {
                    var joints = matjoint.BreathFirstList;

                    anim.FrameCount = 0;

                    int nodeIndex = -1;
                    foreach (var j in joints)
                    {
                        if (j.MaterialAnimation == null)
                            continue;

                        var matAnims = j.MaterialAnimation.List;
                        
                        foreach(var manim in matAnims)
                        {
                            nodeIndex++;
                            var aobj = manim.AnimationObject;
                            if (aobj != null)
                                anim.FrameCount = Math.Max(anim.FrameCount, aobj.EndFrame);

                            var texanim = manim.TextureAnimation;

                            if (texanim == null)
                                continue;
                            var texAOBJ = texanim.AnimationObject;

                            if (texAOBJ == null || texAOBJ.FObjDesc == null)
                                continue;

                            anim.FrameCount = Math.Max(anim.FrameCount, texAOBJ.EndFrame);

                            //TODO: tex anim is a list
                            if (texanim != null)
                            {
                                SBTextureAnimation textureAnim = new SBTextureAnimation();
                                anim.TextureNodes.Add(textureAnim);
                                textureAnim.MeshName = "DOBJ_" + nodeIndex;
                                textureAnim.TextureAttibute = texanim.GXTexMapID.ToString();

                                textureAnim.Keys = DecodeFOBJ(texAOBJ.FObjDesc.ToFOBJ()).Keys;

                                for (int i = 0; i < texanim.ImageCount; i++)
                                {
                                    HSD_TOBJ tobj = new HSD_TOBJ();
                                    tobj.ImageData = texanim.ImageBuffers.Array[i].Data;
                                    if (texanim.TlutCount > i)
                                        tobj.TlutData = texanim.TlutBuffers.Array[i].Data;
                                    var surface = new SBSurface();
                                    surface.Arrays.Add(new MipArray() { Mipmaps = new List<byte[]>() { tobj.GetDecodedImageData() } });
                                    surface.Width = tobj.ImageData.Width;
                                    surface.Height = tobj.ImageData.Height;
                                    surface.PixelFormat = PixelFormat.Bgra;
                                    surface.PixelType = PixelType.UnsignedByte;
                                    surface.InternalFormat = InternalFormat.Rgba;
                                    textureAnim.Surfaces.Add(surface);
                                }
                            }
                        }

                        
                    }

                    SBConsole.WriteLine( nodeIndex);
                }
            }

            return anim;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        private static HSD_FOBJ EncodeFOBJ(SBTransformTrack track)
        {
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
                    InterpolationType = GXInterpolationType.HSD_A_OP_CON
                });
            }
            else
            {
                for (int i = 0; i < track.Keys.Keys.Count; i++)
                {
                    var key = track.Keys.Keys[i];
                    if (i > 0 && track.Keys.Keys[i - 1].InTan != track.Keys.Keys[i - 1].OutTan)
                        keys.Add(new FOBJKey()
                        {
                            Frame = key.Frame,
                            Tan = track.Keys.Keys[i - 1].OutTan,
                            InterpolationType = GXInterpolationType.HSD_A_OP_SLP
                        });

                    if (key.InterpolationType == Scenes.Animation.InterpolationType.Hermite &&
                        prevKey != null &&
                        prevKey.InterpolationType == key.InterpolationType &&
                        prevKey.InTan == key.InTan && prevKey.OutTan == key.OutTan)
                        keys.Add(new FOBJKey()
                        {
                            Frame = key.Frame,
                            Value = key.Value,
                            InterpolationType = GXInterpolationType.HSD_A_OP_SPL0
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
            }

            return FOBJFrameEncoder.EncodeFrames(keys, ToGXTrackType(track.Type));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fobj"></param>
        /// <returns></returns>
        private static SBTransformTrack DecodeFOBJ(HSD_FOBJ fobj)
        {
            SBTrackType trackType = SBTrackType.TranslateX;
            switch (fobj.JointTrackType)
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
            
            var Keys = fobj.GetDecodedKeys();

            // get current frame state
            AnimState prevState = null;
            for (int i = 0; i < Keys.Count; i++)
            {
                var state = GetState(Keys[i].Frame, Keys);

                bool nextSlope = i + 1 < Keys.Count && Keys[i + 1].InterpolationType == GXInterpolationType.HSD_A_OP_SLP;
                
                if (Keys[i].Frame == state.t1)
                {
                    state.t0 = state.t1;
                    state.p0 = state.p1;
                    state.d0 = state.d1;
                }

                if (Keys[i].InterpolationType == GXInterpolationType.HSD_A_OP_SLP)
                    continue;

                if (state.op_intrp == GXInterpolationType.HSD_A_OP_CON || state.op_intrp == GXInterpolationType.HSD_A_OP_KEY)
                {
                    track.AddKey(state.t0, state.p0, InterpolationType.Step);
                }

                if (state.op_intrp == GXInterpolationType.HSD_A_OP_LIN)
                    track.AddKey(state.t0, state.p0, InterpolationType.Linear);

                if (state.op_intrp == GXInterpolationType.HSD_A_OP_SPL
                    || state.op_intrp == GXInterpolationType.HSD_A_OP_SPL0
                    || state.op_intrp == GXInterpolationType.HSD_A_OP_SLP)
                {
                    track.AddKey(state.t0, state.p0, InterpolationType.Hermite,
                            (nextSlope && prevState != null) ? prevState.d1 : state.d0, state.d0);
                }

                prevState = state;
            }
            
            return track;
        }

        public class AnimState
        {
            public float p0 = 0;
            public float p1 = 0;
            public float d0 = 0;
            public float d1 = 0;
            public float t0 = 0;
            public float t1 = 0;
            public GXInterpolationType op_intrp = GXInterpolationType.HSD_A_OP_CON;
            public GXInterpolationType op = GXInterpolationType.HSD_A_OP_CON;
        }

        public static AnimState GetState(float Frame, List<FOBJKey> Keys)
        {
            // register
            float p0 = 0;
            float p1 = 0;
            float d0 = 0;
            float d1 = 0;
            float t0 = 0;
            float t1 = 0;
            GXInterpolationType op_intrp = GXInterpolationType.HSD_A_OP_CON;
            GXInterpolationType op = GXInterpolationType.HSD_A_OP_CON;

            // get current frame state
            for (int i = 0; i < Keys.Count; i++)
            {
                op_intrp = op;
                op = Keys[i].InterpolationType;

                switch (op)
                {
                    case GXInterpolationType.HSD_A_OP_CON:
                        p0 = p1;
                        p1 = Keys[i].Value;
                        if (op_intrp != GXInterpolationType.HSD_A_OP_SLP)
                        {
                            d0 = d1;
                            d1 = 0;
                        }
                        t0 = t1;
                        t1 = Keys[i].Frame;
                        break;
                    case GXInterpolationType.HSD_A_OP_LIN:
                        p0 = p1;
                        p1 = Keys[i].Value;
                        if (op_intrp != GXInterpolationType.HSD_A_OP_SLP)
                        {
                            d0 = d1;
                            d1 = 0;
                        }
                        t0 = t1;
                        t1 = Keys[i].Frame;
                        break;
                    case GXInterpolationType.HSD_A_OP_SPL0:
                        p0 = p1;
                        d0 = d1;
                        p1 = Keys[i].Value;
                        d1 = 0;
                        t0 = t1;
                        t1 = Keys[i].Frame;
                        break;
                    case GXInterpolationType.HSD_A_OP_SPL:
                        p0 = p1;
                        p1 = Keys[i].Value;
                        d0 = d1;
                        d1 = Keys[i].Tan;
                        t0 = t1;
                        t1 = Keys[i].Frame;
                        break;
                    case GXInterpolationType.HSD_A_OP_SLP:
                        d0 = d1;
                        d1 = Keys[i].Tan;
                        break;
                    case GXInterpolationType.HSD_A_OP_KEY:
                        p1 = Keys[i].Value;
                        p0 = Keys[i].Value;
                        break;
                }

                if (t1 > Frame && Keys[i].InterpolationType != GXInterpolationType.HSD_A_OP_SLP)
                    break;

                op_intrp = Keys[i].InterpolationType;
            }
            return new AnimState()
            {
                t0 = t0,
                t1 = t1,
                p0 = p0,
                p1 = p1,
                d0 = d0,
                d1 = d1,
                op = op,
                op_intrp = op_intrp
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="animation"></param>
        /// <param name="skeleton"></param>
        public void ExportSBAnimation(string FileName, SBAnimation animation, SBSkeleton skeleton)
        {
            if (HSDSettings.FigaTree)
            {
                ExportFigaTree(FileName, animation, skeleton);
            }
            else
            {
                ExportAnimJoint(FileName, animation, skeleton);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="animation"></param>
        /// <param name="skeleton"></param>
        private void ExportFigaTree(string FileName, SBAnimation animation, SBSkeleton skeleton)
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
            foreach (var skelnode in skeleton.Bones)
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
                    animTrack.FOBJ = EncodeFOBJ(track);
                    animTrack.DataLength = (short)animTrack.FOBJ.Buffer.Length;
                    animNode.Tracks.Add(animTrack);
                }
            }

            HSD_FigaTree tree = new HSD_FigaTree();
            tree.FrameCount = animation.FrameCount;
            tree.Type = 1;
            tree.Nodes = nodes;

            root.Data = tree;
            file.Save(FileName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="animation"></param>
        /// <param name="skeleton"></param>
        private void ExportAnimJoint(string FileName, SBAnimation animation, SBSkeleton skeleton)
        {
            HSDRawFile file = new HSDRawFile();
            HSDRootNode root = new HSDRootNode();

            if (HSDSettings.RootName == "" || HSDSettings.RootName == null)
                HSDSettings.RootName = System.IO.Path.GetFileNameWithoutExtension(FileName);

            if (HSDSettings.RootName == "" || HSDSettings.RootName == null)
                HSDSettings.RootName = animation.Name;

            root.Name = HSDSettings.RootName;

            if (root.Name == null)
            {
                System.Windows.Forms.MessageBox.Show($"Warning, the root name does not end with \"_figatree\"\n{root.Name}");
            }

            file.Roots.Add(root);

            var joint = new HSD_AnimJoint();
            EncodeAnimJoint(skeleton.Bones[0], joint, animation);
            root.Data = joint;

            file.Save(FileName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bone"></param>
        /// <param name="joint"></param>
        /// <param name="animation"></param>
        private void EncodeAnimJoint(SBBone bone, HSD_AnimJoint joint, SBAnimation animation)
        {
            var node = animation.TransformNodes.Find(e => e.Name == bone.Name);

            if(node != null)
            {
                // encode tracks
                joint.Flags = 1;
                joint.AOBJ = new HSD_AOBJ();
                joint.AOBJ.EndFrame = animation.FrameCount;
                joint.AOBJ.Flags = AOBJ_Flags.ANIM_LOOP;

                if (!HSDSettings.LoopAnimation)
                {
                    joint.AOBJ.Flags = AOBJ_Flags.FIRST_PLAY;
                }
                
                if (node.Tracks.Count == 0)
                    joint.AOBJ.Flags = AOBJ_Flags.NO_ANIM;

                var prev = new HSD_FOBJDesc();

                foreach(var track in node.Tracks)
                {
                    var fobjdesc = new HSD_FOBJDesc();
                    fobjdesc.FromFOBJ(EncodeFOBJ(track));
                    fobjdesc.DataLength = fobjdesc.ToFOBJ().Buffer.Length;

                    if (joint.AOBJ.FObjDesc == null)
                        joint.AOBJ.FObjDesc = fobjdesc;
                    else
                        prev.Next = fobjdesc;

                    prev = fobjdesc;
                }
            }

            // continue adding children
            var prevChild = new HSD_AnimJoint();
            foreach(var c in bone.Children)
            {
                HSD_AnimJoint child = new HSD_AnimJoint();
                EncodeAnimJoint(c, child, animation);
                if (joint.Child == null)
                    joint.Child = child;
                else
                    prevChild.Next = child;
                prevChild = child;
            }
        }

        public static Dictionary<GXInterpolationType, InterpolationType> hsdInterToInter = new Dictionary<GXInterpolationType, InterpolationType>()
        {
            { GXInterpolationType.HSD_A_OP_KEY,  InterpolationType.Constant},
            { GXInterpolationType.HSD_A_OP_SPL,  InterpolationType.Hermite},
            { GXInterpolationType.HSD_A_OP_LIN,  InterpolationType.Linear},
            { GXInterpolationType.HSD_A_OP_CON,  InterpolationType.Step}
        };
        
        private static GXInterpolationType ToGXInterpolation(InterpolationType i)
        {
            switch (i)
            {
                case InterpolationType.Constant:
                    return GXInterpolationType.HSD_A_OP_CON;
                case InterpolationType.Hermite:
                    return GXInterpolationType.HSD_A_OP_SPL;
                case InterpolationType.Linear:
                    return GXInterpolationType.HSD_A_OP_LIN;
                case InterpolationType.Step:
                    return GXInterpolationType.HSD_A_OP_CON; // or key?
                default:
                    return GXInterpolationType.HSD_A_OP_CON;
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
