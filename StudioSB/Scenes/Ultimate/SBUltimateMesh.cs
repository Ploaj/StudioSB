using System.Collections.Generic;
using SSBHLib;
using StudioSB.Rendering.Bounding;
using OpenTK;
using System.ComponentModel;

namespace StudioSB.Scenes.Ultimate
{
    public class SBUltimateMesh : ISBMesh
    {
        public AABoundingBox AABoundingBox { get; set; }
        public OrientedBoundingBox OrientedBoundingBox { get; set; }

        public override int PolyCount => Indices.Count;
        public override int VertexCount => Vertices.Count;

        public List<UltimateVertex> Vertices = new List<UltimateVertex>();
        public List<uint> Indices = new List<uint>();

        // for saving
        public List<UltimateVertexAttribute> ExtraExportAttributes = new List<UltimateVertexAttribute>();

        [Category("Export Options")]
        public bool ExportPosition { get; set; } = true;

        [Category("Export Options")]
        public bool ExportNormal { get; set; } = true;

        [Category("Export Options")]
        public bool ExportTangent { get; set; } = true;

        [Category("Export Options")]
        public bool ExportUVSet1 { get; set; } = false;

        [Category("Export Options")]
        public bool ExportUVSet2 { get; set; } = false;

        [Category("Export Options")]
        public bool ExportUVSet3 { get; set; } = false;

        [Category("Export Options")]
        public bool ExportMap1 { get; set; } = false;

        [Category("Export Options")]
        public bool ExportBake1 { get; set; } = false;

        [Category("Export Options")]
        public bool ExportColorSet1 { get; set; } = false;

        [Category("Export Options")]
        public bool ExportColorSet2 { get; set; } = false;

        [Category("Export Options")]
        public bool ExportColorSet21 { get; set; } = false;

        [Category("Export Options")]
        public bool ExportColorSet22 { get; set; } = false;

        [Category("Export Options")]
        public bool ExportColorSet23 { get; set; } = false;

        [Category("Export Options")]
        public bool ExportColorSet3 { get; set; } = false;

        [Category("Export Options")]
        public bool ExportColorSet4 { get; set; } = false;

        [Category("Export Options")]
        public bool ExportColorSet5 { get; set; } = false;

        [Category("Export Options")]
        public bool ExportColorSet6 { get; set; } = false;

        [Category("Export Options")]
        public bool ExportColorSet7 { get; set; } = false;

        public SBUltimateMesh()
        {
            Visible = true;
        }

        public void EnableAttribute(UltimateVertexAttribute attr)
        {
            switch (attr)
            {
                case UltimateVertexAttribute.Position0: ExportPosition = true; break;
                case UltimateVertexAttribute.Normal0: ExportNormal = true; break;
                case UltimateVertexAttribute.Tangent0: ExportTangent = true; break;
                case UltimateVertexAttribute.uvSet: ExportUVSet1 = true; break;
                case UltimateVertexAttribute.uvSet1: ExportUVSet2 = true; break;
                case UltimateVertexAttribute.uvSet2: ExportUVSet3 = true; break;
                case UltimateVertexAttribute.map1: ExportMap1 = true; break;
                case UltimateVertexAttribute.bake1: ExportBake1 = true; break;
                case UltimateVertexAttribute.colorSet1: ExportColorSet1 = true; break;
                case UltimateVertexAttribute.colorSet2: ExportColorSet2 = true; break;
                case UltimateVertexAttribute.colorSet2_1: ExportColorSet21 = true; break;
                case UltimateVertexAttribute.colorSet2_2: ExportColorSet22 = true; break;
                case UltimateVertexAttribute.colorSet2_3: ExportColorSet23 = true; break;
                case UltimateVertexAttribute.colorSet3: ExportColorSet3 = true; break;
                case UltimateVertexAttribute.colorSet4: ExportColorSet4 = true; break;
                case UltimateVertexAttribute.colorSet5: ExportColorSet5 = true; break;
                case UltimateVertexAttribute.colorSet6: ExportColorSet6 = true; break;
                case UltimateVertexAttribute.colorSet7: ExportColorSet7 = true; break;
                default:
                    ExtraExportAttributes.Add(attr);
                    break;
            }
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
