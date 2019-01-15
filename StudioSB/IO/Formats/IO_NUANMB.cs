using System;
using SSBHLib.Formats.Animation;
using SSBHLib;
using SSBHLib.Tools;
using StudioSB.Scenes.Animation;
using OpenTK;
using StudioSB.Scenes;

namespace StudioSB.IO.Formats
{
    class IO_NUANMB : IImportableAnimation
    {
        public string Name { get { return "Namco Animation Binary"; } }
        public string Extension { get { return ".nuanmb"; } }

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
                foreach (AnimTrack track in animNode.Tracks)
                {
                    object[] Transform = decoder.ReadTrack(track);

                    if (track.Name.Equals("Transform"))
                    {
                        for (int i = 0; i < Transform.Length; i++)
                        {
                            AnimTrackTransform t = (AnimTrackTransform)Transform[i];
                            tfrmAnim.Transform.Keys.Add(new SBAnimKey<Matrix4>()
                            {
                                Frame = i,
                                Value = GetMatrix((AnimTrackTransform)Transform[i]),
                            });
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
                        matAnim.Keys.Keys.Add(new SBAnimKey<Vector4>()
                        {
                            Frame = i,
                            Value = new Vector4(vec.X, vec.Y, vec.Z, vec.W)
                        });
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
                            visAnim.Visibility.Keys.Add(new SBAnimKey<bool>()
                            {
                                Frame = i,
                                Value = (bool)Visibility[i]
                            });
                        }
                    }
                }
                animation.VisibilityNodes.Add(visAnim);
            }
        }
    }
}
