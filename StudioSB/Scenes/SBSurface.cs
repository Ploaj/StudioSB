using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using SFGraphics.GLObjects.Textures;
using SFGraphics.GLObjects.Textures.TextureFormats;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;

namespace StudioSB.Scenes
{
    public class MipArray
    {
        public List<byte[]> Mipmaps = new List<byte[]>();
    }

    /// <summary>
    /// Descripts a texture surface object
    /// </summary>
    public class SBSurface
    {
        [ReadOnly(true), Category("Properties")]
        public string Name { get; set; }

        [ReadOnly(true), Category("Properties")]
        public List<MipArray> Arrays = new List<MipArray>();

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
        public PixelType PixelType { get; set; }
        [ReadOnly(true), Category("Format")]
        public InternalFormat InternalFormat { get; set; }

        [ReadOnly(true), Category("Format")]
        public int ArrayCount { get; set; } = 1;

        [ReadOnly(true), Category("Format")]
        public bool IsCubeMap { get { return Arrays.Count == 6; } }

        [ReadOnly(true), Category("Format")]
        public bool IsSRGB
        {
            get
            {
                return (InternalFormat.ToString().ToLower().Contains("srgb"));
            }
        }

        private Texture renderTexture = null;

        public SBSurface()
        {

        }

        public override string ToString()
        {
            return Name;
        }

        public void RefreshRendering()
        {
            // settings this to null with make the texture reload next time the GetRenderTexture is called
            renderTexture = null;
        }

        public static SBSurface FromBitmap(Bitmap bmp)
        {
            SBSurface surface = new SBSurface();

            surface.Name = "";
            surface.Width = bmp.Width;
            surface.Height = bmp.Height;
            surface.InternalFormat = InternalFormat.Rgba;
            surface.PixelFormat = PixelFormat.Bgra;
            surface.PixelType = PixelType.UnsignedByte;

            var bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var length = bitmapData.Stride * bitmapData.Height;

            byte[] bytes = new byte[length];
            
            Marshal.Copy(bitmapData.Scan0, bytes, 0, length);
            bmp.UnlockBits(bitmapData);

            var mip = new MipArray();
            mip.Mipmaps.Add(bytes);

            surface.Arrays.Add(mip);
            
            return surface;
        }

        /// <summary>
        /// Gets the SFTexture of this surface
        /// </summary>
        /// <returns></returns>
        public Texture GetRenderTexture()
        {
            if(renderTexture == null)
            {
                if(Arrays.Count == 6)
                {
                    var cube = new TextureCubeMap()
                    {
                        MinFilter = TextureMinFilter.Linear,
                        MagFilter = TextureMagFilter.Linear
                    };
                    if (TextureFormatTools.IsCompressed(InternalFormat))
                    {
                        cube.LoadImageData(Width, InternalFormat,
                            Arrays[0].Mipmaps, Arrays[1].Mipmaps, Arrays[2].Mipmaps,
                            Arrays[3].Mipmaps, Arrays[4].Mipmaps, Arrays[5].Mipmaps);
                    }
                    else
                    {
                        var format = new TextureFormatUncompressed((PixelInternalFormat)PixelFormat, PixelFormat, PixelType);
                        
                        cube.LoadImageData(Width, format,
                            Arrays[0].Mipmaps[0], Arrays[1].Mipmaps[0], Arrays[2].Mipmaps[0],
                            Arrays[3].Mipmaps[0], Arrays[4].Mipmaps[0], Arrays[5].Mipmaps[0]);
                    }
                    renderTexture = cube;
                }
                else
                {
                    var sfTex = new Texture2D()
                    {
                        // Set defaults until all the sampler parameters are added.
                        TextureWrapS = TextureWrapMode.Repeat,
                        TextureWrapT = TextureWrapMode.Repeat,
                    };

                    if (TextureFormatTools.IsCompressed(InternalFormat))
                    {
                        // hack
                        // trying to load mipmaps with similar sizes seems to not work
                        var mipTest = new List<byte[]>();
                        int prevsize = 0;
                        foreach (var v in Arrays[0].Mipmaps)
                        {
                            if (v.Length == prevsize)
                                continue;
                            mipTest.Add(v);
                            prevsize = v.Length;
                        }
                        sfTex.LoadImageData(Width, Height, mipTest, InternalFormat);
                    }
                    else
                    {
                        // TODO: Uncompressed mipmaps
                        var format = new TextureFormatUncompressed(PixelInternalFormat.Rgba, PixelFormat, PixelType);
                        sfTex.LoadImageData(Width, Height, Arrays[0].Mipmaps, format);
                    }
                    renderTexture = sfTex;
                }
            }
            return renderTexture;
        }
    }
}
