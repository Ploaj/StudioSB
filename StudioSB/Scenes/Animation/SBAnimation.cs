using System;
using System.Collections.Generic;

namespace StudioSB.Scenes.Animation
{
    public class SBAnimation 
    {
        public float FrameCount { get; set; }

        public List<SBTransformAnimation> TransformNodes = new List<SBTransformAnimation>();
        public List<SBVisibilityAnimation> VisibilityNodes = new List<SBVisibilityAnimation>();
        public List<SBMaterialAnimation> MaterialNodes = new List<SBMaterialAnimation>();

        public void UpdateScene(float Frame, SBScene scene)
        {
            if (scene == null)
                return;

            // Materials
            foreach (SBMaterialAnimation a in MaterialNodes)
            {
                foreach (var material in scene.GetMaterials())
                {
                    if (material.Label.Equals(a.MaterialName))
                    {
                        material.AnimateParam(a.AttributeName, a.Keys.GetValue(Frame));
                    }
                }
            }
            // Visibility
            foreach (SBVisibilityAnimation a in VisibilityNodes)
            {
                foreach (var mesh in scene.GetMeshObjects())
                {
                    // names match with start ignoreing the _VIS tags
                    if (mesh.Name.StartsWith(a.MeshName))
                    {
                        mesh.Visible = a.Visibility.GetValue(Frame);
                    }
                }
            }
            // Bones
            foreach (SBTransformAnimation a in TransformNodes)
            {
                var bone = scene.Skeleton[a.Name];
                if (bone != null)
                    bone.AnimatedTransform = a.Transform.GetValue(Frame);
            }
        }
    }
}
