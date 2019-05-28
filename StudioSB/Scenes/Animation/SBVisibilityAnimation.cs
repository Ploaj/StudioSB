using OpenTK;

namespace StudioSB.Scenes.Animation
{
    public class SBVisibilityAnimation
    {
        public string MeshName { get; set; }

        public SBKeyGroup<bool> Visibility { get; } = new SBKeyGroup<bool>();
    }

    public class SBMaterialAnimation
    {
        public string MaterialName;

        public string AttributeName;

        public SBKeyGroup<Vector4> Keys = new SBKeyGroup<Vector4>();
    }
}
