using OpenTK.Graphics.OpenGL;
using SFGraphics.GLObjects.Textures;
using SFGraphics.GLObjects.Textures.TextureFormats;
using System.Collections.Generic;
using System.Drawing;

namespace StudioSB.Rendering
{
    public class DefaultTextures
    {
        public static DefaultTextures Instance
        {
            get
            {
                if (_defaultTextures == null)
                    _defaultTextures = new DefaultTextures();
                return _defaultTextures;
            }
        }
        private static DefaultTextures _defaultTextures;

        // Default textures.
        public Texture2D defaultWhite = new Texture2D();
        public Texture2D defaultNormal = new Texture2D();
        public Texture2D defaultBlack = new Texture2D();
        public Texture2D defaultPrm = new Texture2D();

        // Font
        public Texture2D renderFont = new Texture2D();

        // Render modes.
        public Texture2D uvPattern = new Texture2D()
        {
            TextureWrapS = TextureWrapMode.Repeat,
            TextureWrapT = TextureWrapMode.Repeat
        };

        // PBR image based lighting.
        public Texture2D iblLut = new Texture2D();
        public TextureCubeMap diffusePbr = new TextureCubeMap();
        public TextureCubeMap specularPbr = new TextureCubeMap();
        public TextureCubeMap blackCube = new TextureCubeMap();

        private Dictionary<string, Texture> TextureByName = new Dictionary<string, Texture>();

        public DefaultTextures()
        {
            LoadBitmap("uvPattern", uvPattern, "DefaultTextures/UVPattern.png");

            LoadBitmap("defaultWhite", defaultWhite, "DefaultTextures/default_White.png");
            LoadBitmap("defaultPrm", defaultPrm, "DefaultTextures/default_Params.tif");
            LoadBitmap("defaultNormal", defaultNormal, "DefaultTextures/default_normal.png");
            LoadBitmap("defaultBlack", defaultBlack, "DefaultTextures/default_black.png");

            LoadBitmap("renderFront", renderFont, "DefaultTextures/render_font.png");
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);

            LoadBitmap("iblBrdf", iblLut, "DefaultTextures/ibl_brdf_lut.png");

            using (var bmp = new Bitmap("DefaultTextures/default_cube_black.png"))
                blackCube.LoadImageData(bmp, 8);
            TextureByName.Add("defaultBlackCube", blackCube);

            LoadSpecularPbr();
            TextureByName.Add("defaultSpecCube", specularPbr);
        }

        public Texture GetTextureByName(string Name)
        {
            // Don't silence this error.
            // It could cause texture type binding mismatches later.
            return TextureByName[Name];
        }

        private void LoadBitmap(string name, Texture2D texture, string path)
        {
            using (var bmp = new Bitmap(path))
            {
                texture.LoadImageData(bmp);
                TextureByName.Add(name, texture);
            }
        }

        private void LoadDiffusePbr()
        {
            diffusePbr = new TextureCubeMap();

            var surfaceData = new List<List<byte[]>>();

            AddIrrFace(surfaceData, "x+");
            AddIrrFace(surfaceData, "x-");
            AddIrrFace(surfaceData, "y+");
            AddIrrFace(surfaceData, "y-");
            AddIrrFace(surfaceData, "z+");
            AddIrrFace(surfaceData, "z-");


            var format = new TextureFormatUncompressed(PixelInternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float);
            diffusePbr.LoadImageData(64, format, surfaceData[0], surfaceData[1], surfaceData[2], surfaceData[3], surfaceData[4], surfaceData[5]);

            // Don't Use mipmaps.
            diffusePbr.MagFilter = TextureMagFilter.Linear;
            diffusePbr.MinFilter = TextureMinFilter.Linear;
        }

        private static void AddIrrFace(List<List<byte[]>> surfaceData, string surface)
        {
            var mipData = System.IO.File.ReadAllBytes($"DefaultTextures/irr {surface}.bin");
            surfaceData.Add(new List<byte[]>() { mipData });
        }

        private void LoadSpecularPbr()
        {
            specularPbr = new TextureCubeMap();
            var surfaceData = new List<List<byte[]>>();

            AddCubeMipmaps(surfaceData, "x+");
            AddCubeMipmaps(surfaceData, "x-");
            AddCubeMipmaps(surfaceData, "y+");
            AddCubeMipmaps(surfaceData, "y-");
            AddCubeMipmaps(surfaceData, "z+");
            AddCubeMipmaps(surfaceData, "z-");

            var format = new TextureFormatUncompressed(PixelInternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float);
            specularPbr.LoadImageData(64, format, surfaceData[0], surfaceData[1], surfaceData[2], surfaceData[3], surfaceData[4], surfaceData[5]);
        }

        private static void AddCubeMipmaps(List<List<byte[]>> surfaceData, string surface)
        {
            var mipmaps = new List<byte[]>();
            for (int mip = 0; mip < 7; mip++)
            {
                var mipData = System.IO.File.ReadAllBytes($"DefaultTextures/spec {surface} {mip}.bin");
                mipmaps.Add(mipData);
            }
            surfaceData.Add(mipmaps);
        }
    }
}
