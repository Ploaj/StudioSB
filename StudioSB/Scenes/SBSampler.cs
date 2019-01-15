using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using SFGraphics.GLObjects.Textures;

namespace StudioSB.Scenes.Ultimate
{
    /// <summary>
    /// This information is present in render texture, but one can have different samplings
    /// </summary>
    public class SBSampler
    {
        public TextureWrapMode WrapS { get; set; } = TextureWrapMode.Repeat;
        public TextureWrapMode WrapT { get; set; } = TextureWrapMode.Repeat;
        public TextureWrapMode WrapR { get; set; } = TextureWrapMode.Repeat;

        /// <summary>
        /// Binds the sampler properties to given render texture
        /// </summary>
        public void Apply(Texture texture)
        {
            texture.TextureWrapS = WrapS;
            texture.TextureWrapT = WrapT;
            texture.TextureWrapR = WrapR;
        }
    }
}
