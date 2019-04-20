using System.Collections.Generic;
using OpenTK;

namespace StudioSB.Scenes.Ultimate
{
    public class SBUltimateModel : ISBModel<SBUltimateMesh<UltimateVertex>>
    {
        public string Name { get; set; }

        public List<SBUltimateMesh<UltimateVertex>> Meshes { get; set; } = new List<SBUltimateMesh<UltimateVertex>>();

        public Vector4 BoundingSphere { get; set; }

        public Vector3 VolumeCenter { get; set; }
        public Vector3 VolumeSize { get; set; }

        public Vector3 OBBPosition { get; set; }
        public Vector3 OBBSize { get; set; }
        public Matrix4 OBBTransform { get; set; }
    }
}
