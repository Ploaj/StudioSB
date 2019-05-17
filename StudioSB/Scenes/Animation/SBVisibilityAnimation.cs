using OpenTK;

namespace StudioSB.Scenes.Animation
{
    public class SBMaterialAnimation
    {
        public string MaterialName;

        public string AttributeName;

        public SBKeyGroup<Vector4> Keys = new SBKeyGroup<Vector4>();
    }

    public class SBVisibilityAnimation
    {
        public string MeshName { get; set; }

        public SBKeyGroup<bool> Visibility { get; } = new SBKeyGroup<bool>();
    }

    public class SBTransformAnimation
    {
        public string Name { get; set; }

        public SBKeyGroup<Matrix4> Transform { get; } = new SBKeyGroup<Matrix4>();
    }
}
