using System.Collections.Generic;

namespace StudioSB.Scenes.Animation
{
    public class SBAnimation 
    {
        public string Name
        {
            get => _name; set
            {
                _name = value;
                RotationOnly = _name.ToLower().Contains("thrown"); // hack to make thrown animations play correctly
            }
        }
        private string _name;
        public float FrameCount { get; set; }

        private bool RotationOnly { get; set; } = false;

        public List<SBTransformAnimation> TransformNodes = new List<SBTransformAnimation>();
        public List<SBVisibilityAnimation> VisibilityNodes = new List<SBVisibilityAnimation>();
        public List<SBMaterialAnimation> MaterialNodes = new List<SBMaterialAnimation>();

        /// <summary>
        /// Applies animation state to given scene
        /// </summary>
        /// <param name="Frame"></param>
        /// <param name="scene"></param>
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
                    // names match with start ignoring the _VIS tags
                    if (a.MeshName != null && mesh.Name.StartsWith(a.MeshName))
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
                {
                    bone.AnimatedTransform = a.GetTransformAt(Frame, bone);
                    if (RotationOnly)
                    {
                        var temp = new SBBone();
                        temp.Transform = bone.AnimatedTransform;
                        temp.Translation = bone.Translation;
                        temp.Scale = bone.Scale;
                        bone.AnimatedTransform = temp.Transform;
                    }
                    if (a.GetTrackValueAt(Frame, SBTrackType.CompensateScale) > 0)
                    {
                        bone.EnableAnimatedCompensateScale = true;
                        bone.AnimatedCompensateScale = a.GetTrackValueAt(Frame, SBTrackType.CompensateScale);
                    }
                    else
                    {
                        bone.EnableAnimatedCompensateScale = false;
                    }
                }
            }
        }
    }
}
