using System.Collections.Generic;
using OpenTK;
using SSBHLib;
using SSBHLib.Formats;

namespace StudioSB.Scenes.Ultimate
{
    public class SKEL_Loader
    {
        public static SBSkeleton Open(string FileName, SBScene Scene)
        {
            ISSBH_File File;
            if (SSBH.TryParseSSBHFile(FileName, out File))
            {
                if (File is SKEL skel)
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
                        bone.Transform = Skel_to_TKMatrix(skel.Transform[b.ID]);
                        idToBone.Add(b.ID, bone);
                        if (b.ParentID == -1)
                            Skeleton.AddRoot(bone);
                        else
                            needParent.Add(bone, b.ParentID);
                    }
                    foreach(var v in needParent)
                    {
                        v.Key.Parent = idToBone[v.Value];
                    }

                    return Skeleton;
                }
            }
            return null;
        }

        public static void Save(string FileName, SBScene Scene)
        {
            var Skeleton = Scene.Skeleton;

            var skelFile = new SKEL();

            skelFile.MajorVersion = 1;
            skelFile.MinorVersion = 0;

            List<SKEL_BoneEntry> BoneEntries = new List<SKEL_BoneEntry>();
            List<SKEL_Matrix> Transforms = new List<SKEL_Matrix>();
            List<SKEL_Matrix> InvTransforms = new List<SKEL_Matrix>();
            List<SKEL_Matrix> WorldTransforms = new List<SKEL_Matrix>();
            List<SKEL_Matrix> InvWorldTransforms = new List<SKEL_Matrix>();

            short index = 0;
            Dictionary<SBBone, short> BoneToIndex = new Dictionary<SBBone, short>();
            var OrderedBones = SortBones(Skeleton.Bones);

            foreach (var bone in OrderedBones)
            {
                BoneToIndex.Add(bone, index);
                var boneentry = new SKEL_BoneEntry();
                boneentry.Name = bone.Name;
                boneentry.Type = bone.Type;
                boneentry.ID = index++;
                boneentry.Type = 1;
                boneentry.ParentID = -1;
                if (bone.Parent != null)// && BoneToIndex.ContainsKey(bone.Parent))
                    boneentry.ParentID = (short)OrderedBones.IndexOf(bone.Parent);
                BoneEntries.Add(boneentry);

                Transforms.Add(TKMatrix_to_Skel(bone.Transform));
                InvTransforms.Add(TKMatrix_to_Skel(bone.Transform.Inverted()));
                WorldTransforms.Add(TKMatrix_to_Skel(bone.WorldTransform));
                InvWorldTransforms.Add(TKMatrix_to_Skel(bone.InvWorldTransform));
            }

            skelFile.BoneEntries = BoneEntries.ToArray();
            skelFile.Transform = Transforms.ToArray();
            skelFile.InvTransform = InvTransforms.ToArray();
            skelFile.WorldTransform = WorldTransforms.ToArray();
            skelFile.InvWorldTransform = InvWorldTransforms.ToArray();

            SSBH.TrySaveSSBHFile(FileName, skelFile);
        }

        private static string[] FighterBoneSet = {
            "Trans",
            "Rot",
            "Hip",
            "Waist",
            "Bust",
            "Neck",
            "Head",
            "ClavicleC",
            "ClavicleR",
            "ShoulderR",
            "ArmR",
            "HandR",
            "HaveR",
            "ClavicleL",
            "ShoulderL",
            "ArmL",
            "HandL",
            "HaveL",
            "LegC",
            "LegL",
            "KneeL",
            "FootL",
            "ToeL",
            "LegR",
            "KneeR",
            "FootR",
            "ToeR",
            "Throw",
        };

        private static List<SBBone> SortBones(IEnumerable<SBBone> boneList)
        {
            var basic = new List<SBBone>();
            var swing = new List<SBBone>();
            var extra = new List<SBBone>();
            var helper = new List<SBBone>();

            // first collect basic strings
            var copy = new List<SBBone>(boneList);

            foreach(var s in FighterBoneSet)
            {
                foreach(var b in copy)
                {
                    if(b.Name.Equals(s))
                    {
                        basic.Add(b);
                        break;
                    }
                }
            }
            foreach(var b in basic)
                copy.Remove(b);

            // every other bone
            foreach(SBBone b in copy)
            {
                if (b.Name.StartsWith("S_"))
                    swing.Add(b);
                else if (b.Name.StartsWith("H_"))
                    helper.Add(b);
                else
                    extra.Add(b);
            }

            var finalList = new List<SBBone>();
            finalList.AddRange(basic);
            finalList.AddRange(swing);
            finalList.AddRange(extra);
            finalList.AddRange(helper);

            return finalList;
        }

        private static Matrix4 Skel_to_TKMatrix(SKEL_Matrix sm)
        {
            return new Matrix4(sm.M11, sm.M12, sm.M13, sm.M14,
                sm.M21, sm.M22, sm.M23, sm.M24,
                sm.M31, sm.M32, sm.M33, sm.M34,
                sm.M41, sm.M42, sm.M43, sm.M44);
        }

        private static SKEL_Matrix TKMatrix_to_Skel(Matrix4 sm)
        {
            var skelmat = new SKEL_Matrix();
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
