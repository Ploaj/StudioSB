using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace StudioSB.Tools
{
    public class TextureFormatInfo
    {
        public static uint GetBPP(InternalFormat format)
        {
            return FormatTable[format].BytesPerPixel;
        }
        public static uint GetBlockWidth(InternalFormat format)
        {
            return FormatTable[format].BlockWidth;
        }
        public static uint GetBlockHeight(InternalFormat format)
        {
            return FormatTable[format].BlockHeight;
        }
        public static uint GetBlockDepth(InternalFormat format)
        {
            return FormatTable[format].BlockDepth;
        }

        private class FormatInfo
        {
            public uint BytesPerPixel { get; private set; }
            public uint BlockWidth { get; private set; }
            public uint BlockHeight { get; private set; }
            public uint BlockDepth { get; private set; }

            public FormatInfo(uint bytesPerPixel, uint blockWidth, uint blockHeight, uint blockDepth)
            {
                BytesPerPixel = bytesPerPixel;
                BlockWidth = blockWidth;
                BlockHeight = blockHeight;
                BlockDepth = blockDepth;
            }
        }

        //TODO: finish
        private static readonly Dictionary<InternalFormat, FormatInfo> FormatTable =
                            new Dictionary<InternalFormat, FormatInfo>()
           {
            { InternalFormat.Rgba32f,       new FormatInfo(16, 1,  1, 1) },
            { InternalFormat.Rgba32i,       new FormatInfo(16, 1,  1, 1) },
            { InternalFormat.Rgba32ui,      new FormatInfo(16, 1,  1, 1) },
            { InternalFormat.Rgba16f,       new FormatInfo(8,  1,  1, 1) },
            { InternalFormat.Rgba16i,       new FormatInfo(8,  1,  1, 1) },
            { InternalFormat.Rgba16ui,      new FormatInfo(8,  1,  1, 1) }, // _SNORM
            { InternalFormat.Rg32f,         new FormatInfo(8,  1,  1, 1) },
            { InternalFormat.Rg32i,         new FormatInfo(8,  1,  1, 1) },
            { InternalFormat.Rg32ui,        new FormatInfo(8,  1,  1, 1) },
            { InternalFormat.Rgba8i,        new FormatInfo(4,  1,  1, 1) },
            { InternalFormat.Rgba8Snorm,    new FormatInfo(4,  1,  1, 1) },
            { InternalFormat.Rgba8ui,       new FormatInfo(4,  1,  1, 1) },
            { InternalFormat.Rgba,          new FormatInfo(4, 1, 1, 1) },
            //{ InternalFormat.Rgba8,       new FormatInfo(4,  1,  1, 1) }, // TEX_FORMAT.R8G8B8A8_UNORM
            //{ InternalFormat.Rgba8Snorm,  new FormatInfo(4,  1,  1, 1) }, // TEX_FORMAT.R8G8B8A8_UNORM_SRGB
            { InternalFormat.R32f,          new FormatInfo(4,  1,  1, 1) }, // TEX_FORMAT.R32G8X24_FLOAT???
            //{ InternalFormat.Rgb8,        new FormatInfo(4, 1,  1, 1) }, // TEX_FORMAT.R8G8_B8G8_UNORM
            //{ InternalFormat.Rgb8,        new FormatInfo(4, 1,  1, 1) }, //TEX_FORMAT.B8G8R8X8_UNORM
            { InternalFormat.Rgb5A1,        new FormatInfo(2, 1,  1, 1) }, // TEX_FORMAT.B5G5R5A1_UNORM
            //{ InternalFormat.Rgba8,       new FormatInfo(4, 1,  1, 1) }, // TEX_FORMAT.B8G8R8A8_UNORM
            //{ InternalFormat.Rgba8Snorm,  new FormatInfo(4, 1,  1, 1) }, // TEX_FORMAT.B8G8R8A8_UNORM_SRGB

            /*{ TEX_FORMAT.R10G10B10A2_UINT,      new FormatInfo(4,  1,  1, 1) },
            { TEX_FORMAT.R10G10B10A2_UNORM,     new FormatInfo(4,  1,  1, 1) },
            { TEX_FORMAT.R32_SINT,              new FormatInfo(4,  1,  1, 1) },
            { TEX_FORMAT.R32_UINT,              new FormatInfo(4,  1,  1, 1) },
            { TEX_FORMAT.R32_FLOAT,             new FormatInfo(4,  1,  1, 1) },
            { TEX_FORMAT.B4G4R4A4_UNORM,        new FormatInfo(2,  1,  1, 1) },
            { TEX_FORMAT.R16G16_FLOAT,          new FormatInfo(4,  1,  1, 1) },
            { TEX_FORMAT.R16G16_SINT,           new FormatInfo(4,  1,  1, 1) },
            { TEX_FORMAT.R16G16_SNORM,          new FormatInfo(4,  1,  1, 1) },
            { TEX_FORMAT.R16G16_UINT,           new FormatInfo(4,  1,  1, 1) },
            { TEX_FORMAT.R16G16_UNORM,          new FormatInfo(4,  1,  1, 1) },
            { TEX_FORMAT.R8G8_SINT,             new FormatInfo(2,  1,  1, 1) },
            { TEX_FORMAT.R8G8_SNORM,            new FormatInfo(2,  1,  1, 1) },
            { TEX_FORMAT.R8G8_UINT,             new FormatInfo(2,  1,  1, 1) },
            { TEX_FORMAT.R8G8_UNORM,            new FormatInfo(2,  1,  1, 1) },
            { TEX_FORMAT.R16_SINT,              new FormatInfo(2,  1,  1, 1) },
            { TEX_FORMAT.R16_SNORM,             new FormatInfo(2,  1,  1, 1) },
            { TEX_FORMAT.R16_UINT,              new FormatInfo(2,  1,  1, 1) },
            { TEX_FORMAT.R16_UNORM,             new FormatInfo(2,  1,  1, 1) },
            { TEX_FORMAT.R8_SINT,               new FormatInfo(1,  1,  1, 1) },
            { TEX_FORMAT.R8_SNORM,              new FormatInfo(1,  1,  1, 1) },
            { TEX_FORMAT.R4G4_UNORM,            new FormatInfo(1,  1,  1, 1) },
            { TEX_FORMAT.R8_UINT,               new FormatInfo(1,  1,  1, 1) },
            { TEX_FORMAT.R8_UNORM,              new FormatInfo(1,  1,  1, 1) },
            { TEX_FORMAT.R11G11B10_FLOAT,       new FormatInfo(4,  1,  1, 1) },
            { TEX_FORMAT.B5G6R5_UNORM,          new FormatInfo(2,  1,  1, 1) },*/
            { InternalFormat.CompressedRgbaS3tcDxt1Ext,             new FormatInfo(8,  4,  4, 1) }, // TEX_FORMAT.BC1_UNORM
            { InternalFormat.CompressedSrgbAlphaS3tcDxt1Ext,        new FormatInfo(8,  4,  4, 1) }, // TEX_FORMAT.BC1_UNORM_SRGB
            //{ TEX_FORMAT.BC2_UNORM,             new FormatInfo(16, 4,  4, 1) },
            //{ TEX_FORMAT.BC2_UNORM_SRGB,        new FormatInfo(16, 4,  4, 1) },
            { InternalFormat.CompressedRgbaS3tcDxt3Ext,             new FormatInfo(16, 4,  4, 1) },
            { InternalFormat.CompressedSrgbAlphaS3tcDxt3Ext,        new FormatInfo(16, 4,  4, 1) },
            //{ TEX_FORMAT.BC4_UNORM,             new FormatInfo(8,  4,  4, 1) },
            //{ TEX_FORMAT.BC4_SNORM,             new FormatInfo(8,  4,  4, 1) },
            { InternalFormat.CompressedRgbaS3tcDxt5Ext,             new FormatInfo(16, 4,  4, 1) },
            { InternalFormat.CompressedSrgbAlphaS3tcDxt5Ext,             new FormatInfo(16, 4,  4, 1) },
            /*{ TEX_FORMAT.BC6H_SF16,             new FormatInfo(16, 4,  4, 1) },*/
            { InternalFormat.CompressedRgbBptcUnsignedFloat,             new FormatInfo(16, 4,  4, 1) },
            { InternalFormat.CompressedRgbaBptcUnorm,             new FormatInfo(16, 4,  4, 1) },
            { InternalFormat.CompressedSrgbAlphaBptcUnorm,        new FormatInfo(16, 4,  4, 1) },

            /*{ TEX_FORMAT.ASTC_4x4_UNORM,        new FormatInfo(16, 4,  4, 1) },
            { TEX_FORMAT.ASTC_4x4_SRGB,         new FormatInfo(16, 4,  4, 1) },
            { TEX_FORMAT.ASTC_5x5_UNORM,        new FormatInfo(16, 5,  5, 1) },
            { TEX_FORMAT.ASTC_6x6_SRGB,         new FormatInfo(16, 6,  6, 1) },
            { TEX_FORMAT.ASTC_8x8_UNORM,        new FormatInfo(16, 8,  8, 1) },
            { TEX_FORMAT.ASTC_8x8_SRGB,         new FormatInfo(16, 8,  8, 1) },
            { TEX_FORMAT.ASTC_10x10_UNORM,      new FormatInfo(16, 10, 10, 1) },
            { TEX_FORMAT.ASTC_10x10_SRGB,       new FormatInfo(16, 10, 10, 1) },
            { TEX_FORMAT.ASTC_12x12_UNORM,      new FormatInfo(16, 12, 12, 1) },
            { TEX_FORMAT.ASTC_12x12_SRGB,       new FormatInfo(16, 12, 12, 1) },
            { TEX_FORMAT.ASTC_5x4_UNORM,        new FormatInfo(16, 5,  4, 1) },
            { TEX_FORMAT.ASTC_5x4_SRGB,         new FormatInfo(16, 5,  4, 1) },
            { TEX_FORMAT.ASTC_6x5_UNORM,        new FormatInfo(16, 6,  5, 1) },
            { TEX_FORMAT.ASTC_6x5_SRGB,         new FormatInfo(16, 6,  5, 1) },
            { TEX_FORMAT.ASTC_8x6_UNORM,        new FormatInfo(16, 8,  6, 1) },
            { TEX_FORMAT.ASTC_8x6_SRGB,         new FormatInfo(16, 8,  6, 1) },
            { TEX_FORMAT.ASTC_10x8_UNORM,       new FormatInfo(16, 10, 8, 1) },
            { TEX_FORMAT.ASTC_10x8_SRGB,        new FormatInfo(16, 10, 8, 1) },
            { TEX_FORMAT.ASTC_12x10_UNORM,      new FormatInfo(16, 12, 10, 1) },
            { TEX_FORMAT.ASTC_12x10_SRGB,       new FormatInfo(16, 12, 10, 1) },
            { TEX_FORMAT.ASTC_8x5_UNORM,        new FormatInfo(16, 8,  5,  1) },
            { TEX_FORMAT.ASTC_8x5_SRGB,         new FormatInfo(16, 8,  5, 1) },
            { TEX_FORMAT.ASTC_10x5_UNORM,       new FormatInfo(16, 10, 5, 1) },
            { TEX_FORMAT.ASTC_10x5_SRGB,        new FormatInfo(16, 10, 5, 1) },
            { TEX_FORMAT.ASTC_10x6_UNORM,       new FormatInfo(16, 10, 6, 1) },
            { TEX_FORMAT.ASTC_10x6_SRGB,        new FormatInfo(16, 10, 6, 1) },
            { TEX_FORMAT.ETC1,                  new FormatInfo(4, 1, 1, 1) },
            { TEX_FORMAT.ETC1_A4,               new FormatInfo(8, 1, 1, 1) },
            { TEX_FORMAT.HIL08,                 new FormatInfo(16, 1, 1, 1) },
            { TEX_FORMAT.L4,                    new FormatInfo(4, 1, 1, 1) },
            { TEX_FORMAT.LA4,                   new FormatInfo(4, 1, 1, 1) },
            { TEX_FORMAT.L8,                    new FormatInfo(8, 1, 1, 1) },
            { TEX_FORMAT.LA8,                   new FormatInfo(16, 1, 1, 1) },
            { TEX_FORMAT.A4,                    new FormatInfo(4, 1,  1, 1) },
            { TEX_FORMAT.A8_UNORM,              new FormatInfo(8,  1,  1, 1) },

            { TEX_FORMAT.D16_UNORM,            new FormatInfo(2, 1, 1, 1)        },
            { TEX_FORMAT.D24_UNORM_S8_UINT,    new FormatInfo(4, 1, 1, 1)        },
            { TEX_FORMAT.D32_FLOAT,            new FormatInfo(4, 1, 1, 1)        },
            { TEX_FORMAT.D32_FLOAT_S8X24_UINT, new FormatInfo(8, 1, 1, 1) }*/
        };
    }
}
