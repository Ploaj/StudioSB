using System.Collections.Generic;
using System.Text;
using OpenTK;
using System.IO;
using StudioSB.Scenes;
using System.Text.RegularExpressions;
using StudioSB.Scenes.Animation;
using System;

namespace StudioSB.IO.Formats
{
    public class IO_SMD : IExportableAnimation, IImportableAnimation
    {
        public string Name { get; } = "Source Model";
        public string Extension { get; } = ".smd";
        public object Settings { get; } = null;

        #region Classes
        
        private class SMD
        {
            public int Version = 1;

            public List<SMDNode> nodes = new List<SMDNode>();
            public List<SMDSkeletonFrame> skeleton = new List<SMDSkeletonFrame>();
            public List<SMDTriangle> triangles = new List<SMDTriangle>();

            public void Save(string FileName)
            {
                StringBuilder o = new StringBuilder();

                o.AppendLine($"version {Version}");

                // Nodes----------------
                o.AppendLine("nodes");
                foreach (var node in nodes)
                {
                    o.AppendLine($"{node.ID} \"{node.Name}\" {node.ParentID}");
                }
                o.AppendLine("end");

                //Skeleton ------------------------
                o.AppendLine("skeleton");
                foreach (var g in skeleton)
                {
                    o.AppendLine($"time {g.time}");
                    foreach (var skel in g.skeletons)
                    {
                        o.AppendLine($"{skel.BoneID} {skel.Position.X} {skel.Position.Y} {skel.Position.Z} {skel.Rotation.X} {skel.Rotation.Y} {skel.Rotation.Z}");
                    }
                }
                o.AppendLine("end");

                //Triangles ------------------------
                o.AppendLine("triangles");
                foreach (var tri in triangles)
                {
                    o.AppendLine(tri.Material);
                    o.AppendLine(tri.vertex1.ToString());
                    o.AppendLine(tri.vertex2.ToString());
                    o.AppendLine(tri.vertex3.ToString());
                }
                o.AppendLine("end");

                File.WriteAllText(FileName, o.ToString());
            }

            public void Open(string FileName)
            {
                string[] lines = File.ReadAllLines(FileName);
                string CurrentSection = "";
                SMDSkeletonFrame CurrentSkeleton = null;
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] args = Regex.Replace(lines[i].Trim(), @"\s+", " ").Split(' ');

                    if (args[0].Equals("end"))
                    {
                        CurrentSection = "";
                    }
                    else
                    if (args[0].Equals("time"))
                    {
                        if (CurrentSkeleton != null)
                            skeleton.Add(CurrentSkeleton);
                        CurrentSkeleton = new SMDSkeletonFrame();
                        int.TryParse(args[1], out CurrentSkeleton.time);
                    }
                    else
                    if (args[0].Equals("nodes"))
                    {
                        CurrentSection = args[0];
                    }
                    else
                    if (args[0].Equals("skeleton"))
                    {
                        CurrentSection = args[0];
                    }
                    else
                    if (args[0].Equals("triangles"))
                    {
                        CurrentSection = args[0];
                    }
                    else
                    {
                        switch (CurrentSection)
                        {
                            case "nodes":
                                var node = new SMDNode();
                                node.Name = args[1].Replace("\"", "");
                                node.ID = int.Parse(args[0]);
                                node.ParentID = int.Parse(args[0]);
                                nodes.Add(node);
                                break;
                            case "skeleton":
                                var skel = new SMDSkeleton();
                                skel.BoneID = int.Parse(args[0]);
                                skel.Position = new Vector3(float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
                                skel.Rotation = new Vector3(float.Parse(args[4]), float.Parse(args[5]), float.Parse(args[6]));
                                CurrentSkeleton.skeletons.Add(skel);
                                break;
                            case "triangles":
                                var triangle = new SMDTriangle();
                                triangle.Material = args[0];
                                triangles.Add(triangle);
                                triangle.vertex1.Parse(Regex.Replace(lines[i + 1].Trim(), @"\s+", " ").Split(' '));
                                triangle.vertex1.Parse(Regex.Replace(lines[i + 2].Trim(), @"\s+", " ").Split(' '));
                                triangle.vertex1.Parse(Regex.Replace(lines[i + 3].Trim(), @"\s+", " ").Split(' '));
                                i += 3;
                                break;
                        }
                    }
                }

                if (CurrentSkeleton != null)
                    skeleton.Add(CurrentSkeleton);
            }
        }

        private class SMDNode
        {
            public string Name;
            public int ID;
            public int ParentID;
        }

        private class SMDSkeletonFrame
        {
            public int time = 0;
            public List<SMDSkeleton> skeletons = new List<SMDSkeleton>();
        }

        private class SMDSkeleton
        {
            public int BoneID;
            public Vector3 Position;
            public Vector3 Rotation;
        }

        private class SMDTriangle
        {
            public string Material;
            public SMDVertex vertex1 = new SMDVertex();
            public SMDVertex vertex2 = new SMDVertex();
            public SMDVertex vertex3 = new SMDVertex();
        }

        private class SMDVertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 UV;
            public int[] Bones;
            public float[] Weights;

            public override string ToString()
            {
                return $"{0} {Position.X} {Position.Y} {Position.Z} {Normal.X} {Normal.Y} {Normal.Z} {UV.X} {UV.Y} {Bones.Length}" + GetWeights();
            }

            private string GetWeights()
            {
                StringBuilder weightlist = new StringBuilder();
                for (int i = 0; i < Bones.Length; i++)
                {
                    weightlist.Append($" {Bones[i]} {Weights[i]}");
                }
                return weightlist.ToString();
            }

            public void Parse(string[] args)
            {
                Position = new Vector3(float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
                Normal = new Vector3(float.Parse(args[4]), float.Parse(args[5]), float.Parse(args[6]));
                UV = new Vector2(float.Parse(args[7]), float.Parse(args[8]));

                int InfluenceCount = int.Parse(args[9]);
                Bones = new int[InfluenceCount];
                Weights = new float[InfluenceCount];

                for (int i = 0; i < InfluenceCount * 2; i += 2)
                {
                    int boneIndex = int.Parse(args[10 + i]);
                    float weight = float.Parse(args[10 + i + 1]);
                    Bones[i / 2] = boneIndex;
                    Weights[i / 2] = weight;
                }

            }
        }

        #endregion
        

        #region Export Animation

        public void ExportSBAnimation(string FileName, SBAnimation animation, SBSkeleton skeleton)
        {
            SMD file = new SMD();
            
            var bonelist = new List<SBBone>(skeleton.Bones);
            foreach (var bone in bonelist)
            {
                file.nodes.Add(new SMDNode() { Name = bone.Name, ID = bonelist.IndexOf(bone), ParentID = bonelist.IndexOf(bone.Parent) });
            }

            for(int i = 0; i < animation.FrameCount; i++)
            {
                var group = new SMDSkeletonFrame();
                group.time = i;
                file.skeleton.Add(group);

                foreach (var bone in bonelist)
                {
                    bool found = false;
                    foreach(var animbone in animation.TransformNodes)
                    {
                        if (animbone.Name.Equals(bone.Name))
                        {
                            var tempBone = new SBBone();
                            tempBone.Transform = animbone.GetTransformAt(i, skeleton);
                            group.skeletons.Add(new SMDSkeleton() { BoneID = bonelist.IndexOf(bone), Position = tempBone.Translation, Rotation = tempBone.RotationEuler });
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        group.skeletons.Add(new SMDSkeleton() { BoneID = bonelist.IndexOf(bone), Position = bone.Translation, Rotation = bone.RotationEuler });
                    }
                }

            }

            file.Save(FileName);
        }

        public SBAnimation ImportSBAnimation(string FileName, SBSkeleton skeleton)
        {
            SMD smd = new SMD();
            smd.Open(FileName);

            SBAnimation animation = new SBAnimation();
            animation.Name = Path.GetFileNameWithoutExtension(FileName);
            Dictionary<int, SBTransformAnimation> idToAnim = new Dictionary<int, SBTransformAnimation>();

            foreach (var node in smd.nodes)
            {
                var nodeAnim = new SBTransformAnimation() { Name = node.Name };
                idToAnim.Add(node.ID, nodeAnim);

                animation.TransformNodes.Add(nodeAnim);
            }

            var frameCount = 0;
            foreach(var v in smd.skeleton)
            {
                frameCount = Math.Max(v.time, frameCount);
                foreach (var node in v.skeletons)
                {
                    var animNode = idToAnim[node.BoneID];

                    animNode.AddKey(v.time, node.Position.X, SBTrackType.TranslateX);
                    animNode.AddKey(v.time, node.Position.Y, SBTrackType.TranslateY);
                    animNode.AddKey(v.time, node.Position.Z, SBTrackType.TranslateZ);
                    animNode.AddKey(v.time, node.Rotation.X, SBTrackType.RotateX);
                    animNode.AddKey(v.time, node.Rotation.Y, SBTrackType.RotateY);
                    animNode.AddKey(v.time, node.Rotation.Z, SBTrackType.RotateZ);
                }
            }

            animation.FrameCount = frameCount;

            return animation;
        }

        #endregion
    }
}