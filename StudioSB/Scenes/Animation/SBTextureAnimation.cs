using SFGraphics.GLObjects.Textures;
using System.Collections.Generic;

namespace StudioSB.Scenes.Animation
{
    /// <summary>
    /// 
    /// </summary>
    public class SBTextureAnimation
    {
        public string MeshName { get; set; }

        public string TextureAttibute { get; set; }

        public SBKeyGroup<float> Keys = new SBKeyGroup<float>();

        public List<SBSurface> Surfaces = new List<SBSurface>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public Texture GetTextureAt(float frame)
        {
            if (Surfaces.Count == 0)
                return null;

            var key = Keys.GetValue(frame);

            if (key > Surfaces.Count || key < 0)
                return Surfaces[0].GetRenderTexture();

            return Surfaces[(int)key].GetRenderTexture();
        }
    }
}
