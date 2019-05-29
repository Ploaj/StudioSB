using SSBHLib.Formats.Animation;
using SSBHLib;
using SSBHLib.Tools;
using StudioSB.Scenes.Animation;
using OpenTK;
using StudioSB.Scenes;
using System.Collections.Generic;
using System.Linq;
using System;

namespace StudioSB.IO.Formats
{
    class IO_NUANMB : IImportableAnimation, IExportableAnimation
    {
        public string Name { get { return "Namco Animation Binary"; } }
        public string Extension { get { return ".nuanmb"; } }

        public object Settings => null;

        public SBAnimation ImportSBAnimation(string FileName, SBSkeleton skeleton)
        {
            SBAnimation anim = new SBAnimation();

            ISSBH_File File;
            if (SSBH.TryParseSSBHFile(FileName, out File))
            {
                if (File is ANIM animation)
                {
                    anim.FrameCount = animation.FrameCount;

                    foreach (var group in animation.Animations)
                    {
                        if (group.Type == ANIM_TYPE.Visibilty)
                        {
                            ReadVisibilityAnimations(animation, group, anim);
                        }
                        if (group.Type == ANIM_TYPE.Transform)
                        {
                            ReadTransformAnimations(animation, group, anim);
                        }
                        if (group.Type == ANIM_TYPE.Material)
                        {
                            ReadMaterialAnimations(animation, group, anim);
                        }
                    }
                }
            }

            return anim;
        }

        private void ReadTransformAnimations(ANIM animFile, AnimGroup animGroup, SBAnimation animation)
        {
            var decoder = new SSBHAnimTrackDecoder(animFile);

            foreach (AnimNode animNode in animGroup.Nodes)
            {
                SBTransformAnimation tfrmAnim = new SBTransformAnimation()
                {
                    Name = animNode.Name
                };
                SBTransformTrack X = new SBTransformTrack(SBTrackType.TranslateX);
                SBTransformTrack Y = new SBTransformTrack(SBTrackType.TranslateY);
                SBTransformTrack Z = new SBTransformTrack(SBTrackType.TranslateZ);
                SBTransformTrack RX = new SBTransformTrack(SBTrackType.RotateX);
                SBTransformTrack RY = new SBTransformTrack(SBTrackType.RotateY);
                SBTransformTrack RZ = new SBTransformTrack(SBTrackType.RotateZ);
                SBTransformTrack SX = new SBTransformTrack(SBTrackType.ScaleX);
                SBTransformTrack SY = new SBTransformTrack(SBTrackType.ScaleY);
                SBTransformTrack SZ = new SBTransformTrack(SBTrackType.ScaleZ);
                SBTransformTrack CompensateScale = new SBTransformTrack(SBTrackType.CompensateScale);
                tfrmAnim.Tracks.AddRange(new SBTransformTrack[] { X, Y, Z, RX, RY, RZ, SX, SY, SZ, CompensateScale });

                foreach (AnimTrack track in animNode.Tracks)
                {
                    object[] Transform = decoder.ReadTrack(track);

                    if (track.Name.Equals("Transform"))
                    {
                        for (int i = 0; i < Transform.Length; i++)
                        {
                            AnimTrackTransform t = (AnimTrackTransform)Transform[i];

                            SBBone transform = new SBBone();
                            transform.Transform = GetMatrix((AnimTrackTransform)Transform[i]);
                            X.AddKey(i, transform.X);
                            Y.AddKey(i, transform.Y);
                            Z.AddKey(i, transform.Z);
                            RX.AddKey(i, transform.RX);
                            RY.AddKey(i, transform.RY);
                            RZ.AddKey(i, transform.RZ);
                            SX.AddKey(i, transform.SX);
                            SY.AddKey(i, transform.SY);
                            SZ.AddKey(i, transform.SZ);
                            CompensateScale.AddKey(i, 0);// t.CompensateScale);
                        }
                    }
                }
                animation.TransformNodes.Add(tfrmAnim);
            }
        }


        private static Matrix4 GetMatrix(AnimTrackTransform Transform)
        {
            return Matrix4.CreateScale(Transform.SX, Transform.SY, Transform.SZ) *
                Matrix4.CreateFromQuaternion(new Quaternion(Transform.RX, Transform.RY, Transform.RZ, Transform.RW)) *
                Matrix4.CreateTranslation(Transform.X, Transform.Y, Transform.Z);
        }

        private void ReadMaterialAnimations(ANIM animFile, AnimGroup animGroup, SBAnimation animation)
        {
            var decoder = new SSBHAnimTrackDecoder(animFile);

            foreach (AnimNode animNode in animGroup.Nodes)
            {
                foreach (AnimTrack track in animNode.Tracks)
                {
                    SBMaterialAnimation matAnim = new SBMaterialAnimation()
                    {
                        MaterialName = animNode.Name,
                        AttributeName = track.Name
                    };
                    object[] MaterialAnim = decoder.ReadTrack(track);

                    // only get vectors for now
                    if (MaterialAnim == null || MaterialAnim.Length == 0 || MaterialAnim[0] == null || MaterialAnim[0].GetType() != typeof(AnimTrackCustomVector4))
                    {
                        continue;
                    }
                    animation.MaterialNodes.Add(matAnim);
                    for (int i = 0; i < MaterialAnim.Length; i++)
                    {
                        var vec = (AnimTrackCustomVector4)MaterialAnim[i];
                        matAnim.Keys.AddKey(i, new Vector4(vec.X, vec.Y, vec.Z, vec.W));
                    }
                }
            }
        }

        private void ReadVisibilityAnimations(ANIM animFile, AnimGroup animGroup, SBAnimation animation)
        {
            var decoder = new SSBHAnimTrackDecoder(animFile);
            foreach (AnimNode animNode in animGroup.Nodes)
            {
                SBVisibilityAnimation visAnim = new SBVisibilityAnimation()
                {
                    MeshName = animNode.Name
                };
                foreach (AnimTrack track in animNode.Tracks)
                {
                    if (track.Name.Equals("Visibility"))
                    {
                        object[] Visibility = decoder.ReadTrack(track);

                        for (int i = 0; i < Visibility.Length; i++)
                        {
                            visAnim.Visibility.AddKey(i, (bool)Visibility[i]);
                        }
                    }
                }
                animation.VisibilityNodes.Add(visAnim);
            }
        }

        public void ExportSBAnimation(string FileName, SBAnimation animation, SBSkeleton skeleton)
        {
            SSBHAnimTrackEncoder encoder = new SSBHAnimTrackEncoder(animation.FrameCount);

            var animNodes = animation.TransformNodes.OrderBy(e => e.Name, StringComparer.Ordinal);

            foreach (var node in animNodes)
            {
                List<object> transforms = new List<object>();

                for(int i = 0; i < animation.FrameCount; i++)
                {
                    transforms.Add(MatrixToTransform(node.GetTransformAt(i, skeleton)));
                }

                encoder.AddTrack(node.Name, "Transform", ANIM_TYPE.Transform, transforms);
            }


            var visNodes = animation.VisibilityNodes.OrderBy(e => e.MeshName, StringComparer.Ordinal);

            foreach (var node in visNodes)
            {
                List<object> visibilities = new List<object>();

                for (int i = 0; i < animation.FrameCount; i++)
                {
                    visibilities.Add(node.Visibility.GetValue(i));
                }

                encoder.AddTrack(node.MeshName, "Visibility", ANIM_TYPE.Visibilty, visibilities);
            }

            encoder.Save(FileName);
        }

        private static AnimTrackTransform MatrixToTransform(Matrix4 matrix)
        {
            SBBone temp = new SBBone();
            temp.Transform = matrix;

            var t = new AnimTrackTransform()
            {
                X = temp.X,
                Y = temp.Y,
                Z = temp.Z,
                RX = temp.RotationQuaternion.X,
                RY = temp.RotationQuaternion.Y,
                RZ = temp.RotationQuaternion.Z,
                RW = temp.RotationQuaternion.W,
                SX = temp.SX,
                SY = temp.SY,
                SZ = temp.SZ
            };

            return t;
        }
    }
}
