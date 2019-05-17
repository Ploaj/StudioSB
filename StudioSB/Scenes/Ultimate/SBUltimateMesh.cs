using System.Collections.Generic;
using SSBHLib;
using StudioSB.Rendering.Bounding;

namespace StudioSB.Scenes.Ultimate
{
    public class SBUltimateMesh : ISBMesh
    {
        public BoundingSphere BoundingSphere { get; set; }
        public AABoundingBox AABoundingBox { get; set; }
        public OrientedBoundingBox OrientedBoundingBox { get; set; }

        public override int PolyCount => Indices.Count;
        public override int VertexCount => Vertices.Count;

        public List<UltimateVertex> Vertices = new List<UltimateVertex>();
        public List<uint> Indices = new List<uint>();

        // for saving
        public List<UltimateVertexAttribute> ExportAttributes = new List<UltimateVertexAttribute>();

        public SBUltimateMesh()
        {
            Visible = true;
        }

        public void CalculateBounding()
        {
            List<Vector3> meshVertices = new List<Vector3>(Vertices.Count);
            
            foreach(var v in Vertices)
            {
                meshVertices.Add(v.Position0);
            }

            BoundingSphere = new BoundingSphere(meshVertices);
            AABoundingBox = new AABoundingBox(meshVertices);
            OrientedBoundingBox = new OrientedBoundingBox(meshVertices);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
