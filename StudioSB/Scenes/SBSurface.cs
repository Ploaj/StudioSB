using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using SFGraphics.GLObjects.Textures;
using SFGraphics.GLObjects.Textures.TextureFormats;
using System.ComponentModel;

namespace StudioSB.Scenes
{
    /// <summary>
    /// Descripts a texture surface object
    /// </summary>
    public class SBSurface
    {
        [ReadOnly(true), Category("Properties")]
        public string Name { get; set; }

        [ReadOnly(true), Category("Properties")]
        public List<byte[]> Mipmaps = new List<byte[]>();

        [ReadOnly(true), Category("Dimensions")]
        public int Width { get; set; }
        [ReadOnly(true), Category("Dimensions")]
        public int Height { get; set; }
        [ReadOnly(true), Category("Dimensions")]
        public int Depth { get; set; }

        [ReadOnly(true), Category("Format")]
        public TextureTarget TextureTarget { get; set; }
        [ReadOnly(true), Category("Format")]
        public PixelFormat PixelFormat { get; set; }
        [ReadOnly(true), Category("Format")]
        public InternalFormat InternalFormat { get; set; }

        [ReadOnly(true), Category("Format")]
        public bool IsSRGB { get; set; }

        private Texture renderTexture = null;

        public SBSurface()
        {

        }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Gets the SFTexture of this surface
        /// </summary>
        /// <returns></returns>
        public Texture CreateRenderTexture()
        {
            if(renderTexture == null)
            {
                var sfTex = new Texture2D()
                {
                    // Set defaults until all the sampler parameters are added.
                    TextureWrapS = TextureWrapMode.Repeat,
                    TextureWrapT = TextureWrapMode.Repeat
                };

                if (TextureFormatTools.IsCompressed(InternalFormat))
                {
                    sfTex.LoadImageData(Width, Height, Mipmaps, InternalFormat);
                }
                else
                {
                    // TODO: Uncompressed mipmaps.
                    var format = new TextureFormatUncompressed((PixelInternalFormat)PixelFormat, PixelFormat, PixelType.UnsignedByte);
                    sfTex.LoadImageData(Width, Height, Mipmaps[0], format);
                }
                renderTexture = sfTex;
            }
            return renderTexture;
        }
    }
}
