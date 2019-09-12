using System.Collections.Generic;
using StudioSB.Scenes;
using OpenTK;

namespace StudioSB.IO.Models
{
    public class IOModel
    {
        public string Name;
        public SBSkeleton Skeleton;
        public List<IOMesh> Meshes = new List<IOMesh>();
        public List<IOMaterial> Materials = new List<IOMaterial>();
        public List<SBSurface> Textures = new List<SBSurface>();

        public bool HasSkeleton { get { return Skeleton != null; } }
        public bool HasMeshes { get { return Meshes != null && Meshes.Count != 0; } }
        public bool HasMaterials { get { return Materials != null && Materials.Count > 0; } }
        public bool HasTextures { get { return Textures != null && Textures.Count > 0; } }

        /// <summary>
        /// Transform single bound vertices by the inverse of their bound bone
        /// </summary>
        public void InvertSingleBinds()
        {
            if (Skeleton == null)
                return;
            foreach (var mesh in Meshes)
            {
                for(int v = 0; v < mesh.Vertices.Count; v++)
                {
                    var vertex = mesh.Vertices[v];
                    for (int i = 0; i < 4; i++)
                    {
                        if (vertex.BoneWeights[i] == 1)
                        {
                            vertex.Position = Vector3.TransformPosition(vertex.Position, Skeleton.Bones[(int)vertex.BoneIndices[i]].InvWorldTransform);
                            vertex.Normal = Vector3.TransformNormal(vertex.Normal, Skeleton.Bones[(int)vertex.BoneIndices[i]].InvWorldTransform);
                        }
                    }
                    mesh.Vertices[v] = vertex;
                }
            }
        }
            /// <summary>
            /// converts the rigging to a new skeleton
            /// </summary>
            /// <param name="newSkeleton"></param>
        public void ConvertToSkeleton(SBSkeleton newSkeleton)
        {
            if (Skeleton == null || newSkeleton == null)
                return;
            Dictionary<int, int> oldToNew = new Dictionary<int, int>();
            Dictionary<string, int> boneNameToPosition = new Dictionary<string, int>();
            for(int i = 0; i < Skeleton.Bones.Length; i++)
            {
                var boneName = Skeleton.Bones[i].Name;
                if (!boneNameToPosition.ContainsKey(boneName))
                    boneNameToPosition.Add(boneName, i);
            }
            for(int i = 0; i < newSkeleton.Bones.Length; i++)
            {
                if (boneNameToPosition.ContainsKey(newSkeleton.Bones[i].Name))
                {
                    oldToNew.Add(boneNameToPosition[newSkeleton.Bones[i].Name], i);
                }
            }

            // Correct Vertex Rigging
            HashSet<string> mismatchedBones = new HashSet<string>();
            foreach(var mesh in Meshes)
            {
                for(int i = 0; i < mesh.Vertices.Count; i++)
                {
                    var vertex = mesh.Vertices[i];

                    for(int b = 0; b < 4; b++)
                    {
                        if (vertex.BoneWeights[b] > 0 && oldToNew.ContainsKey((int)vertex.BoneIndices[b]))
                            vertex.BoneIndices[b] = oldToNew[(int)vertex.BoneIndices[b]];
                        else
                        {
                            var name = newSkeleton.Bones[(int)vertex.BoneIndices[b]].Name;
                            if (!mismatchedBones.Contains(name))
                            {
                                SBConsole.WriteLine($"Warning: missmatched bone {name}");
                                mismatchedBones.Add(name);
                            }
                        }
                    }

                    mesh.Vertices[i] = vertex;
                }
            }

            // accept new skeleton
            Skeleton = newSkeleton;
        }
    }
}
