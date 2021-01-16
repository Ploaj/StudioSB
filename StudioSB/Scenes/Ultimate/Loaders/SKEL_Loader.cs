using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using SELib;
using SSBHLib;
using SSBHLib.Formats;
using StudioSB.Tools;

namespace StudioSB.Scenes.Ultimate
{
    public class SKEL_Loader
    {
        public static SBSkeleton Open(string FileName, SBScene Scene)
        {
            if (Ssbh.TryParseSsbhFile(FileName, out SsbhFile File))
            {
                if (File is Skel skel)
                {
                    var Skeleton = new SBSkeleton();
                    Scene.Skeleton = Skeleton;

                    Dictionary<int, SBBone> idToBone = new Dictionary<int, SBBone>();
                    Dictionary<SBBone, int> needParent = new Dictionary<SBBone, int>();
                    foreach (var b in skel.BoneEntries)
                    {
                        SBBone bone = new SBBone();
                        bone.Name = b.Name;
                        bone.Type = b.Type;
                        bone.Transform = skel.Transform[b.Id].ToOpenTK();
                        idToBone.Add(b.Id, bone);
                        if (b.ParentId == -1)
                            Skeleton.AddRoot(bone);
                        else
                            needParent.Add(bone, b.ParentId);
                    }
                    foreach (var v in needParent)
                    {
                        v.Key.Parent = idToBone[v.Value];
                    }

                    return Skeleton;
                }
            }
            return null;
        }

        public static void Save(string fileName, SBScene scene, string sourceSkelPath)
        {
            var Skeleton = scene.Skeleton;

            var skelFile = new Skel();

            skelFile.MajorVersion = 1;
            skelFile.MinorVersion = 0;

            List<SkelBoneEntry> BoneEntries = new List<SkelBoneEntry>();
            List<Matrix4x4> Transforms = new List<Matrix4x4>();
            List<Matrix4x4> InvTransforms = new List<Matrix4x4>();
            List<Matrix4x4> WorldTransforms = new List<Matrix4x4>();
            List<Matrix4x4> InvWorldTransforms = new List<Matrix4x4>();

            // Attempt to match bone order to an existing SKEL if possible.
            var sortedBones = Skeleton.Bones;
            if (!string.IsNullOrEmpty(sourceSkelPath))
            {
                sortedBones = GetBoneOrderBasedOnReference(Skeleton.Bones, sourceSkelPath);
            }

            short index = 0;
            foreach (var bone in sortedBones)
            {
                var boneEntry = new SkelBoneEntry
                {
                    Name = bone.Name,
                    Type = bone.Type,
                    Id = index++
                };
                boneEntry.Type = 1;
                boneEntry.ParentId = -1;
                if (bone.Parent != null)
                    boneEntry.ParentId = (short)Array.IndexOf(sortedBones, bone.Parent);
                BoneEntries.Add(boneEntry);

                Transforms.Add(bone.Transform.ToSsbh());
                InvTransforms.Add(bone.Transform.Inverted().ToSsbh());
                WorldTransforms.Add(bone.WorldTransform.ToSsbh());
                InvWorldTransforms.Add(bone.InvWorldTransform.ToSsbh());
            }

            skelFile.BoneEntries = BoneEntries.ToArray();
            skelFile.Transform = Transforms.ToArray();
            skelFile.InvTransform = InvTransforms.ToArray();
            skelFile.WorldTransform = WorldTransforms.ToArray();
            skelFile.InvWorldTransform = InvWorldTransforms.ToArray();

            Ssbh.TrySaveSsbhFile(fileName, skelFile);
        }

        private static SBBone[] GetBoneOrderBasedOnReference(SBBone[] targetBones, string sourceSkelPath)
        {
            // TODO: This is quite inefficient.
            if (Ssbh.TryParseSsbhFile(sourceSkelPath, out SsbhFile sourceSkelFile))
            {
                if (sourceSkelFile is Skel sourceSkel)
                {
                    // Match the existing bone order when possible.
                    // Ignore any bones in the source SKEL not found in targetBones.
                    var sortedResult = new List<SBBone>();
                    foreach (var sourceBone in sourceSkel.BoneEntries)
                    {
                        var targetBone = targetBones.FirstOrDefault(b => b.Name == sourceBone.Name);
                        if (targetBone != null)
                            sortedResult.Add(targetBone);
                    }

                    // Bones not found in the source SKEL should be placed at the end.
                    foreach (var targetBone in targetBones)
                    {
                        if (sourceSkel.BoneEntries.FirstOrDefault(b => b.Name == targetBone.Name) == null)
                            sortedResult.Add(targetBone);
                    }

                    return sortedResult.ToArray();
                }
            }

            return targetBones;
        }
    }
}
