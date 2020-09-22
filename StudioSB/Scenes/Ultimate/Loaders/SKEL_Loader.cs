using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using SELib;
using SSBHLib;
using SSBHLib.Formats;

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
                        bone.Transform = SkelToTKMatrix(skel.Transform[b.Id]);
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
            List<SkelMatrix> Transforms = new List<SkelMatrix>();
            List<SkelMatrix> InvTransforms = new List<SkelMatrix>();
            List<SkelMatrix> WorldTransforms = new List<SkelMatrix>();
            List<SkelMatrix> InvWorldTransforms = new List<SkelMatrix>();

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

                Transforms.Add(TKMatrixToSkel(bone.Transform));
                InvTransforms.Add(TKMatrixToSkel(bone.Transform.Inverted()));
                WorldTransforms.Add(TKMatrixToSkel(bone.WorldTransform));
                InvWorldTransforms.Add(TKMatrixToSkel(bone.InvWorldTransform));
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

        private static Matrix4 SkelToTKMatrix(SkelMatrix sm)
        {
            return new Matrix4(sm.M11, sm.M12, sm.M13, sm.M14,
                sm.M21, sm.M22, sm.M23, sm.M24,
                sm.M31, sm.M32, sm.M33, sm.M34,
                sm.M41, sm.M42, sm.M43, sm.M44);
        }

        private static SkelMatrix TKMatrixToSkel(Matrix4 sm)
        {
            var skelmat = new SkelMatrix();
            skelmat.M11 = sm.M11;
            skelmat.M12 = sm.M12;
            skelmat.M13 = sm.M13;
            skelmat.M14 = sm.M14;
            skelmat.M21 = sm.M21;
            skelmat.M22 = sm.M22;
            skelmat.M23 = sm.M23;
            skelmat.M24 = sm.M24;
            skelmat.M31 = sm.M31;
            skelmat.M32 = sm.M32;
            skelmat.M33 = sm.M33;
            skelmat.M34 = sm.M34;
            skelmat.M41 = sm.M41;
            skelmat.M42 = sm.M42;
            skelmat.M43 = sm.M43;
            skelmat.M44 = sm.M44;
            return skelmat;
        }
    }
}
