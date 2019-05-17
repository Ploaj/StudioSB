using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using SFGraphics.GLObjects.Textures;
using SFGraphics.GLObjects.Textures.TextureFormats;

namespace StudioSB.Scenes
{
    /// <summary>
    /// Descripts a texture surface object
    /// </summary>
    public class SBSurface
    {
        public string Name { get; set; }

        public List<byte[]> Mipmaps = new List<byte[]>();

        public int Width { get; set; }
        public int Height { get; set; }
        public int Depth { get; set; }

        public TextureTarget TextureTarget { get; set; }
        public PixelFormat PixelFormat { get; set; }
        public InternalFormat InternalFormat { get; set; }

        public bool IsSRGB { get; set; }

        private Texture renderTexture = null;

        public SBSurface()
        {

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
