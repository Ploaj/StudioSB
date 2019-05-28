using StudioSB.IO.Models;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using System.IO;
using StudioSB.Scenes;
using System.Text.RegularExpressions;
using StudioSB.Scenes.Animation;

namespace StudioSB.IO.Formats
{
    public class IO_SMD : IExportableModelType, IImportableModelType, IExportableAnimation
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
                                skel.Position = new Vector3(float.Parse(args[4]), float.Parse(args[5]), float.Parse(args[6]));
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

        #region Import Model

        public IOModel ImportIOModel(string FileName)
        {
            IOModel model = new IOModel();
            SBSkeleton skeleton = new SBSkeleton();
            
            Dictionary<int, SBBone> indexToBone = new Dictionary<int, SBBone>();
            Dictionary<string, IOMesh> materialToMesh = new Dictionary<string, IOMesh>();
            List<SBBone> PostProcessBones = new List<SBBone>();
            List<int> boneParents = new List<int>();
            
            string[] lines = File.ReadAllLines(FileName);
            string CurrentSection = "";
            for(int i = 0; i < lines.Length; i++)
            {
                string[] args = Regex.Replace(lines[i].Trim(), @"\s+", " ").Split(' ');

                if (args[0].Equals("end"))
                {
                    CurrentSection = "";
                }
                else
                if (args[0].Equals("time"))
                {

                }else
                if (args[0].Equals("nodes"))
                {
                    CurrentSection = args[0];
                }else
                if (args[0].Equals("skeleton"))
                {
                    CurrentSection = args[0];
                }else
                if (args[0].Equals("triangles"))
                {
                    CurrentSection = args[0];
                }
                else
                {
                    switch (CurrentSection)
                    {
                        case "nodes":
                            var bone = new SBBone();
                            string bname = "";
                            for(int k = 0; k < args.Length - 2; k++)
                                bname += args[1+k];
                            bone.Name = bname.Replace("\"", "");
                            boneParents.Add(int.Parse(args[args.Length - 1]));
                            PostProcessBones.Add(bone);
                            indexToBone.Add(int.Parse(args[0]), bone);
                            break;
                        case "skeleton":
                            var skel = PostProcessBones[int.Parse(args[0])];
                            skel.Transform = Matrix4.Identity;
                            skel.X = float.Parse(args[1]);
                            skel.Y = float.Parse(args[2]);
                            skel.Z = float.Parse(args[3]);
                            skel.RX = float.Parse(args[4]);
                            skel.RY = float.Parse(args[5]);
                            skel.RZ = float.Parse(args[6]);
                            break;
                        case "triangles":
                            string material = args[0];
                            if (!materialToMesh.ContainsKey(material))
                            {
                                var iomesh = new IOMesh();
                                iomesh.HasPositions = true;
                                iomesh.HasNormals = true;
                                iomesh.HasUV0 = true;
                                iomesh.HasBoneWeights = true;
                                iomesh.Name = material;
                                materialToMesh.Add(material, iomesh);
                            }
                            var mesh = materialToMesh[material];
                            mesh.Vertices.Add(ReadVertex(Regex.Replace(lines[i + 1].Trim(), @"\s+", " ").Split(' ')));
                            mesh.Vertices.Add(ReadVertex(Regex.Replace(lines[i + 2].Trim(), @"\s+", " ").Split(' ')));
                            mesh.Vertices.Add(ReadVertex(Regex.Replace(lines[i + 3].Trim(), @"\s+", " ").Split(' ')));
                            i += 3;
                            break;
                    }
                }
            }

            //PostProcessBones
            int boneIndex = 0;
            foreach(var bone in PostProcessBones)
            {
                if(boneParents[boneIndex] == -1)
                {
                    skeleton.AddRoot(bone);
                }
                else
                {
                    bone.Parent = indexToBone[boneParents[boneIndex]];
                }
                boneIndex++;
            }

            model.Skeleton = skeleton;
            // finalize meshes
            foreach(var pair in materialToMesh)
            {
                model.Meshes.Add(pair.Value);
                pair.Value.Optimize();
                SBConsole.WriteLine($"Imported {pair.Key} from SMD");

                //finalize rigging
                if(indexToBone.Count > 0)
                for(int i = 0; i < pair.Value.Vertices.Count; i++)
                {
                    var vertex = pair.Value.Vertices[i];
                    vertex.BoneIndices.X = model.Skeleton.IndexOfBone(indexToBone[(int)vertex.BoneIndices.X]);
                    vertex.BoneIndices.Y = model.Skeleton.IndexOfBone(indexToBone[(int)vertex.BoneIndices.Y]);
                    vertex.BoneIndices.Z = model.Skeleton.IndexOfBone(indexToBone[(int)vertex.BoneIndices.Z]);
                    vertex.BoneIndices.W = model.Skeleton.IndexOfBone(indexToBone[(int)vertex.BoneIndices.W]);
                    pair.Value.Vertices[i] = vertex;
                }

            }

            return model;
        }

        private static IOVertex ReadVertex(string[] args)
        {
            var vertex = new IOVertex()
            {
                Position = new Vector3(float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3])),
                Normal = new Vector3(float.Parse(args[4]), float.Parse(args[5]), float.Parse(args[6])),
                UV0 = new Vector2(float.Parse(args[7]), 1-float.Parse(args[8])),
            };
            vertex.Color = Vector4.One;

            int InfluenceCount = int.Parse(args[9]);
            
            for (int i = 0; i < InfluenceCount * 2; i+=2)
            {
                int boneIndex = int.Parse(args[10 + i]);
                float weight = float.Parse(args[10 + i + 1]);
                if(i / 2 == 0)
                {
                    vertex.BoneIndices.X = boneIndex;
                    vertex.BoneWeights.X = weight;
                }
                if (i / 2 == 1)
                {
                    vertex.BoneIndices.Y = boneIndex;
                    vertex.BoneWeights.Y = weight;
                }
                if (i / 2 == 2)
                {
                    vertex.BoneIndices.Z = boneIndex;
                    vertex.BoneWeights.Z = weight;
                }
                if (i / 2 == 3)
                {
                    vertex.BoneIndices.W = boneIndex;
                    vertex.BoneWeights.W = weight;
                }
            }
            
            return vertex;
        }

#endregion

        #region Export Model
        public void ExportIOModel(string FileName, IOModel model)
        {
            ExportIOModelAsSMD(FileName, model);
        }

        public static void ExportIOModelAsSMD(string FileName, IOModel Model)
        {
            SMD file = new SMD();

            if (Model.HasSkeleton)
            {
                var bonelist = new List<SBBone>(Model.Skeleton.Bones);
                var frame = new SMDSkeletonFrame();
                file.skeleton.Add(frame);
                frame.time = 0;
                foreach (var bone in bonelist)
                {
                    file.nodes.Add(new SMDNode() { Name = bone.Name, ID = bonelist.IndexOf(bone), ParentID = bonelist.IndexOf(bone.Parent)});
                    frame.skeletons.Add(new SMDSkeleton() { BoneID = bonelist.IndexOf(bone), Position = bone.Translation, Rotation = bone.RotationEuler });
                }
            }

            if (Model.HasMeshes)
            {
                Dictionary<string, int> UniqueMeshNames = new Dictionary<string, int>();
                
                foreach (var mesh in Model.Meshes)
                {
                    if (!UniqueMeshNames.ContainsKey(mesh.Name))
                        UniqueMeshNames.Add(mesh.Name, 0);

                    string Name = mesh.Name + (UniqueMeshNames[mesh.Name] == 0 ? "" : "_" + UniqueMeshNames[mesh.Name]);
                    UniqueMeshNames[mesh.Name]++;

                    for (int i = 0; i < mesh.Indices.Count; i+=3)
                    {
                        var triangle = new SMDTriangle();
                        triangle.Material = Name;
                        triangle.vertex1 = IOVertexToSMDVertex(mesh.Vertices[(int)mesh.Indices[i+0]]);
                        triangle.vertex2 = IOVertexToSMDVertex(mesh.Vertices[(int)mesh.Indices[i+1]]);
                        triangle.vertex3 = IOVertexToSMDVertex(mesh.Vertices[(int)mesh.Indices[i+2]]);
                        file.triangles.Add(triangle);
                    }
                }
            }

            file.Save(FileName);
        }

        private static SMDVertex IOVertexToSMDVertex(IOVertex iovertex)
        {
            var smdvertex = new SMDVertex();

            smdvertex.Position = iovertex.Position;
            smdvertex.Normal = iovertex.Normal;
            smdvertex.UV = iovertex.UV0;

            int weightCount = CountWeights(iovertex.BoneWeights);
            smdvertex.Bones = new int[weightCount];
            smdvertex.Weights = new float[weightCount];
            for(int i = 0;i < weightCount; i++)
            {
                smdvertex.Bones[i] = (int)iovertex.BoneIndices[i];
                smdvertex.Weights[i] = iovertex.BoneWeights[i];
            }

            return smdvertex;
        }

        private static int CountWeights(Vector4 Weight)
        {
            int c = 0;
            if (Weight.X != 0) c += 1; else return c;
            if (Weight.Y != 0) c += 1; else return c;
            if (Weight.Z != 0) c += 1; else return c;
            if (Weight.W != 0) c += 1; else return c;
            return c;
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

        #endregion
    }
}