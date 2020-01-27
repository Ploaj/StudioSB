using StudioSB.Scenes;
using StudioSB.Scenes.Animation;
using SELib;
using System.Collections.Generic;

namespace StudioSB.IO.Formats
{
    public class IO_SEANIM : IExportableAnimation, IImportableAnimation
    {
        public string Name { get; } = "SEAnim";
        public string Extension { get; } = ".seanim";
        public object Settings { get; } = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="animation"></param>
        /// <param name="skeleton"></param>
        public void ExportSBAnimation(string FileName, SBAnimation animation, SBSkeleton skeleton)
        {
            SEAnim seOut = new SEAnim(); //init new SEAnim

            foreach (var node in animation.TransformNodes) //iterate through each node
            {
                for (int i = 0; i < animation.FrameCount; i++)
                {
                    SBBone temp = new SBBone();

                    temp.Transform = node.GetTransformAt(i, skeleton);

                    seOut.AddTranslationKey(node.Name, i, temp.X, temp.Y, temp.Z);
                    seOut.AddRotationKey(node.Name, i, temp.RotationQuaternion.X, temp.RotationQuaternion.Y, temp.RotationQuaternion.Z, temp.RotationQuaternion.W);
                    seOut.AddScaleKey(node.Name, i, temp.SX, temp.SY, temp.SZ);
                }
            }

            seOut.Write(FileName); //write it!
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="skeleton"></param>
        /// <returns></returns>
        public SBAnimation ImportSBAnimation(string FileName, SBSkeleton skeleton)
        {
            SEAnim anim = SEAnim.Read(FileName);

            var sbAnim = new SBAnimation();

            sbAnim.FrameCount = anim.FrameCount;
            sbAnim.Name = anim.DeltaTagName;

            Dictionary<string, SBTransformAnimation> nameToAnim = new Dictionary<string, SBTransformAnimation>();
            
            foreach (var v in anim.AnimationPositionKeys)
            {
                if (!nameToAnim.ContainsKey(v.Key))
                {
                    SBTransformAnimation an = new SBTransformAnimation();
                    an.Name = v.Key;
                    nameToAnim.Add(v.Key, an);
                    sbAnim.TransformNodes.Add(an);
                }

                var a = nameToAnim[v.Key];

                foreach (var k in v.Value)
                {
                    if (k.Data is SELib.Utilities.Vector2 vc)
                    {
                        a.AddKey(k.Frame, (float)vc.X, SBTrackType.TranslateX, InterpolationType.Linear);
                        a.AddKey(k.Frame, (float)vc.Y, SBTrackType.TranslateY, InterpolationType.Linear);
                    }
                    if (k.Data is SELib.Utilities.Vector3 vc3)
                    {
                        a.AddKey(k.Frame, (float)vc3.X, SBTrackType.TranslateX, InterpolationType.Linear);
                        a.AddKey(k.Frame, (float)vc3.Y, SBTrackType.TranslateY, InterpolationType.Linear);
                        a.AddKey(k.Frame, (float)vc3.Z, SBTrackType.TranslateZ, InterpolationType.Linear);
                    }
                }
            }

            foreach (var v in anim.AnimationRotationKeys)
            {
                if (!nameToAnim.ContainsKey(v.Key))
                {
                    SBTransformAnimation an = new SBTransformAnimation();
                    an.Name = v.Key;
                    nameToAnim.Add(v.Key, an);
                    sbAnim.TransformNodes.Add(an);
                }

                var a = nameToAnim[v.Key];

                foreach (var k in v.Value)
                {
                    if(k.Data is SELib.Utilities.Quaternion q)
                    {
                        var euler = Tools.CrossMath.ToEulerAngles(new OpenTK.Quaternion((float)q.X, (float)q.Y, (float)q.Z, (float)q.W));
                        
                        a.AddKey(k.Frame, euler.X, SBTrackType.RotateX, InterpolationType.Linear);
                        a.AddKey(k.Frame, euler.Y, SBTrackType.RotateY, InterpolationType.Linear);
                        a.AddKey(k.Frame, euler.Z, SBTrackType.RotateZ, InterpolationType.Linear);
                    }
                    if (k.Data is SELib.Utilities.Vector3 vc)
                    {
                        a.AddKey(k.Frame, (float)vc.X, SBTrackType.RotateX, InterpolationType.Linear);
                        a.AddKey(k.Frame, (float)vc.Y, SBTrackType.RotateY, InterpolationType.Linear);
                        a.AddKey(k.Frame, (float)vc.Z, SBTrackType.RotateZ, InterpolationType.Linear);
                    }
                }
            }


            foreach (var v in anim.AnimationScaleKeys)
            {
                if (!nameToAnim.ContainsKey(v.Key))
                {
                    SBTransformAnimation an = new SBTransformAnimation();
                    an.Name = v.Key;
                    nameToAnim.Add(v.Key, an);
                    sbAnim.TransformNodes.Add(an);
                }

                var a = nameToAnim[v.Key];

                foreach (var k in v.Value)
                {
                    if (k.Data is SELib.Utilities.Vector3 vc3)
                    {
                        a.AddKey(k.Frame, (float)vc3.X, SBTrackType.ScaleX, InterpolationType.Linear);
                        a.AddKey(k.Frame, (float)vc3.Y, SBTrackType.ScaleY, InterpolationType.Linear);
                        a.AddKey(k.Frame, (float)vc3.Z, SBTrackType.ScaleZ, InterpolationType.Linear);
                    }
                }
            }

            return sbAnim;
        }
    }
}
