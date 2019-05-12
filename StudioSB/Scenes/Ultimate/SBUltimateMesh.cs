using System.Collections.Generic;
using SSBHLib;
using StudioSB.Rendering.Bounding;

namespace StudioSB.Scenes.Ultimate
{
    public class SBUltimateMesh<T> : ISBMesh
    {
        public BoundingSphere BoundingSphere { get; set; }
        public AABoundingBox AABoundingBox { get; set; }
        public OrientedBoundingBox OrientedBoundingBox { get; set; }

        public override int PolyCount => Indices.Count;
        public override int VertexCount => Vertices.Count;

        public List<T> Vertices = new List<T>();
        public List<uint> Indices = new List<uint>();

        // for saving
        public List<UltimateVertexAttribute> ExportAttributes = new List<UltimateVertexAttribute>();

        public SBUltimateMesh()
        {
            Visible = true;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
