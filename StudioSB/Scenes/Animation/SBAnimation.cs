using StudioSB.Scenes.Melee;
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
        public List<SBTextureAnimation> TextureNodes = new List<SBTextureAnimation>();

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
            foreach (var material in scene.GetMaterials())
            {
                material.ClearAnimations();
                foreach (SBMaterialAnimation a in MaterialNodes)
                {
                    if (material.Label.Equals(a.MaterialName))
                    {
                        material.AnimateParam(a.AttributeName, a.Keys.GetValue(Frame));
                    }
                }
            }

            foreach (var mesh in scene.GetMeshObjects())
            {
                // Visibility
                foreach (SBVisibilityAnimation a in VisibilityNodes)
                {
                    // names match with start ignoring the _VIS tags
                    if (a.MeshName != null && mesh.Name.StartsWith(a.MeshName))
                    {
                        mesh.Visible = a.Visibility.GetValue(Frame);
                    }
                }
                // Textures
                foreach (SBTextureAnimation texanim in TextureNodes)
                {
                    if (texanim.MeshName != null && mesh.Name.StartsWith(texanim.MeshName))
                    {
                        if(mesh is SBHsdMesh hsdMesh)
                        {
                            var mat = hsdMesh.Material as SBHsdMaterial;

                            mat.AnimateParam(texanim.TextureAttibute, texanim.GetTextureAt(Frame));
                        }
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
                    if (ApplicationSettings.EnableCompensateScale && a.GetTrackValueAt(Frame, SBTrackType.CompensateScale) != 0)
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

        /// <summary>
        /// 
        /// </summary>
        public void ConvertRotationKeysToEuler()
        {
            foreach(var v in TransformNodes)
            {
                v.ConvertRotationToEuler((int)System.Math.Ceiling(FrameCount));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Optimize()
        {
            foreach(var v in TransformNodes)
            {
                foreach(var track in v.Tracks)
                {
                    track.Optimize();
                }
            }
        }
    }
}
