using System.Collections.Generic;
using OpenTK;
using StudioSB.Rendering.Bounding;

namespace StudioSB.Scenes.Ultimate
{
    public class SBUltimateModel : ISBModel<SBUltimateMesh>
    {
        public string Name { get; set; }

        public List<SBUltimateMesh> Meshes { get; set; } = new List<SBUltimateMesh>();

        public Vector4 BoundingSphere { get; set; }
        
        public AABoundingBox AABoundingBox { get; set; }
        public OrientedBoundingBox OrientedBoundingBox { get; set; }
    }
}
