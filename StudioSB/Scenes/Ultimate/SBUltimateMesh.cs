using System.Collections.Generic;
using OpenTK;
using SSBHLib.Tools;

namespace StudioSB.Scenes.Ultimate
{
    public class SBUltimateMesh<T> : ISBMesh
    {
        public Vector4 BoundingSphere { get; set; }

        public override int PolyCount => Indices.Count;
        public override int VertexCount => Vertices.Count;

        public List<T> Vertices = new List<T>();
        public List<uint> Indices = new List<uint>();

        // for saving
        public List<MESHAttribute> ExportAttributes = new List<MESHAttribute>();

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
