using OpenTK.Graphics.OpenGL;
using SFGraphics.GLObjects.Textures;
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
            TextureByName.Add("blackCube", blackCube);

            LoadDiffusePbr();
            LoadSpecularPbr();         
        }

        public Texture GetTextureByName(string Name)
        {
            if (TextureByName.ContainsKey(Name))
                return TextureByName[Name];
            else
                return defaultWhite;
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
            for (int surface = 0; surface < 6; surface++)
            {
                var mipData = System.IO.File.ReadAllBytes($"DefaultTextures/diffuseSdr{surface}{0}.bin");
                surfaceData.Add(new List<byte[]>() { mipData });
            }
            diffusePbr.LoadImageData(128, InternalFormat.CompressedRgbaS3tcDxt1Ext, surfaceData[0], surfaceData[1], surfaceData[2], surfaceData[3], surfaceData[4], surfaceData[5]);

            // Don't Use mipmaps.
            diffusePbr.MagFilter = TextureMagFilter.Linear;
            diffusePbr.MinFilter = TextureMinFilter.Linear;
        }

        private void LoadSpecularPbr()
        {
            specularPbr = new TextureCubeMap();
            var surfaceData = new List<List<byte[]>>();
            for (int surface = 0; surface < 6; surface++)
            {
                var mipmaps = new List<byte[]>();
                for (int mip = 0; mip < 10; mip++)
                {
                    var mipData = System.IO.File.ReadAllBytes($"DefaultTextures/specularSdr{surface}{mip}.bin");
                    mipmaps.Add(mipData);
                }
                surfaceData.Add(mipmaps);
            }
            specularPbr.LoadImageData(512, InternalFormat.CompressedRgbaS3tcDxt1Ext, surfaceData[0], surfaceData[1], surfaceData[2], surfaceData[3], surfaceData[4], surfaceData[5]);
        }
    }
}
