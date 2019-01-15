using System;
using System.Collections.Generic;
using OpenTK;
using SFGraphics.Utils;

namespace StudioSB.IO.Models
{
    public struct IOVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector2 UV0;
        public Vector2 UV1;
        public Vector2 UV2;
        public Vector2 UV3;
        public Vector4 Color;
        public Vector4 BoneIndices;
        public Vector4 BoneWeights;
    }

    public class IOMesh
    {
        public string Name { get; set; }
        public List<IOVertex> Vertices { get; } = new List<IOVertex>();
        public List<uint> Indices { get; } = new List<uint>();

        public int MaterialIndex = -1;

        public bool HasPositions { get; set; } = false;
        public bool HasNormals { get; set; } = false;
        public bool HasUV0 { get; set; } = false;
        public bool HasUV1 { get; set; } = false;
        public bool HasUV2 { get; set; } = false;
        public bool HasUV3 { get; set; } = false;
        public bool HasColor { get; set; } = false;
        public bool HasBoneWeights { get; set; } = false;

        /// <summary>
        /// Generates Tangents and Bitangents for the vertices
        /// </summary>
        public void GenerateTangentsAndBitangents()
        {
            for(int i = 0; i < Indices.Count; i+=3)
            {
                var vert1 = Vertices[(int)Indices[i]];
                var vert2 = Vertices[(int)Indices[i+1]];
                var vert3 = Vertices[(int)Indices[i+2]];

                Vector3 tan = Vector3.Zero;
                Vector3 bitan = Vector3.Zero;
                VectorUtils.GenerateTangentBitangent(vert1.Position, vert2.Position, vert3.Position, 
                    vert1.UV0, vert2.UV0, vert3.UV0, out tan, out bitan);

                vert1.Tangent += tan;
                vert2.Tangent += tan;
                vert3.Tangent += tan;

                Vertices[(int)Indices[i]] = vert1;
                Vertices[(int)Indices[i+1]] = vert2;
                Vertices[(int)Indices[i+2]] = vert3;
            }
        }

        /// <summary>
        /// Optimized the vertex index size by removing duplicate vertices
        /// </summary>
        public void Optimize()
        {
            Indices.Clear();

            List<IOVertex> NewVertices = new List<IOVertex>();
            Dictionary<IOVertex, uint> vertexToIndex = new Dictionary<IOVertex, uint>();

            foreach(var vertex in Vertices)
            {
                if (!vertexToIndex.ContainsKey(vertex))
                {
                    vertexToIndex.Add(vertex, (uint)NewVertices.Count);
                    NewVertices.Add(vertex);
                }
                Indices.Add(vertexToIndex[vertex]);
            }

            SBConsole.WriteLine($"Optimized {Name} {Vertices.Count} -> {NewVertices.Count}");

            Vertices.Clear();
            Vertices.AddRange(NewVertices);
        }
    }
}
