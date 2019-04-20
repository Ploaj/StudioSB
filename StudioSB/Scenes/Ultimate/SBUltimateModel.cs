using System.Collections.Generic;
using OpenTK;
using StudioSB.Rendering.Bounding;

namespace StudioSB.Scenes.Ultimate
{
    public class SBUltimateModel : ISBModel<SBUltimateMesh<UltimateVertex>>
    {
        public string Name { get; set; }

        public List<SBUltimateMesh<UltimateVertex>> Meshes { get; set; } = new List<SBUltimateMesh<UltimateVertex>>();

        public Vector4 BoundingSphere { get; set; }
        
        public AABoundingBox AABoundingBox { get; set; }
        public OrientedBoundingBox OrientedBoundingBox { get; set; }
    }
}
