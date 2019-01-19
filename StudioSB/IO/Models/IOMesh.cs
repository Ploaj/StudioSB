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
        public Vector3 Bitangent;
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
            var positions = GetPositions();
            var normals = GetNormals();
            var uvs = GetUvs();
            var signedIndices = GetSignedIndices();
            TriangleListUtils.CalculateTangentsBitangents(positions, normals, uvs, signedIndices,
                out Vector3[] tangents, out Vector3[] bitangents);

            for (int i = 0; i < Vertices.Count; i++)
            {
                var vertex = Vertices[i];
                vertex.Tangent = tangents[i];
                vertex.Tangent.Normalize();
                vertex.Bitangent = bitangents[i];
                Vertices[i] = vertex;
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

        private List<Vector3> GetPositions()
        {
            var values = new List<Vector3>();
            foreach (var vertex in Vertices)
            {
                values.Add(vertex.Position);
            }
            return values;
        }

        private List<Vector3> GetNormals()
        {
            var values = new List<Vector3>();
            foreach (var vertex in Vertices)
            {
                values.Add(vertex.Normal);
            }
            return values;
        }

        private List<Vector2> GetUvs()
        {
            var values = new List<Vector2>();
            foreach (var vertex in Vertices)
            {
                values.Add(vertex.UV0);
            }
            return values;
        }

        private List<int> GetSignedIndices()
        {
            var values = new List<int>();
            foreach (var index in Indices)
            {
                values.Add((int)index);
            }
            return values;
        }
    }
}
