using StudioSB.Scenes;
using StudioSB.Tools;
using System;
using System.IO;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace StudioSB.IO.Formats
{
    public class IO_DDS
    {

        public static SBSurface Import(string FileName)
        {
            SBSurface surface = new SBSurface();

            using (BinaryReaderExt reader = new BinaryReaderExt(new FileStream(FileName, FileMode.Open)))
            {
                DDS_Header header = new DDS_Header();
                header.Read(reader);

                surface.Name = Path.GetFileNameWithoutExtension(FileName);
                surface.Width = header.dwWidth;
                surface.Height = header.dwHeight;
                if (header.dwFlags.HasFlag(DDSD.DEPTH))
                    surface.Depth = header.dwDepth;
                else
                    surface.Depth = 1;

                if (header.ddspf.dwFourCC == 0x31545844)
                {
                    surface.InternalFormat = InternalFormat.CompressedRgbaS3tcDxt1Ext;
                }else
                if (header.ddspf.dwFourCC == 0x30315844)
                {
                    surface.InternalFormat = DXGItoInternal(header.DXT10Header.dxgiFormat);
                    if(surface.InternalFormat == 0)
                    {
                        System.Windows.Forms.MessageBox.Show("DDS format not supported " + header.DXT10Header.dxgiFormat);

                        return null;
                    }

                }
                else
                {

                    if(((FourCC_DXGI)header.ddspf.dwFourCC) == FourCC_DXGI.D3DFMT_A32B32G32R32F)
                    {
                        surface.InternalFormat = InternalFormat.Rgba32f;
                        surface.PixelFormat = PixelFormat.Rgba;
                        surface.PixelType = PixelType.Float;
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("DDS format not supported " + header.ddspf.dwFourCC.ToString("X"));
                        return null;
                    }
                }


                // TODO: read other mips
                int w = surface.Width;
                int h = surface.Height;
                //SBConsole.WriteLine(header.dwCaps.ToString() + " " + header.dwCaps2.ToString() + " " + header.dwFlags.ToString());
                for(int array = 0; array < (header.dwCaps2.HasFlag(DDSCAPS2.CUBEMAP_ALLFACES) ? 6 : 1); array++)
                {
                    w = surface.Width;
                    h = surface.Height;
                    var mip = new MipArray();

                    for (int i = 0; i < (header.dwFlags.HasFlag(DDSD.MIPMAPCOUNT) ? header.dwMipMapCount : 1); i++)
                    {
                        var mipSize = Math.Max(1, ((w + 3) / 4)) * Math.Max(1, ((h + 3) / 4) ) * (int)TextureFormatInfo.GetBPP(surface.InternalFormat);

                        if (mipSize % TextureFormatInfo.GetBPP(surface.InternalFormat) != 0)
                            mipSize += (int)(TextureFormatInfo.GetBPP(surface.InternalFormat) - (mipSize % TextureFormatInfo.GetBPP(surface.InternalFormat)));

                        var data = reader.ReadBytes(mipSize);

                        mip.Mipmaps.Add(data);
                        w /= 2;
                        h /= 2;
                    }

                    surface.Arrays.Add(mip);
                }
                if (reader.Position != reader.BaseStream.Length)
                    SBConsole.WriteLine("Warning: error reading dds " + reader.Position.ToString("X"));
            }

            

            return surface;
        }

        public static void Export(string fileName, SBSurface surface)
        {
            if (!internalFormatToDXGI.ContainsKey(surface.InternalFormat) && !internalFormatToD3D.ContainsKey(surface.InternalFormat))
            {
                System.Windows.Forms.MessageBox.Show("Unsupported DDS export format " + surface.InternalFormat.ToString());
                return;
            }
            var Header = new DDS_Header()
            {
                dwFlags = (DDSD.CAPS | DDSD.HEIGHT | DDSD.WIDTH | DDSD.PIXELFORMAT | DDSD.LINEARSIZE),
                dwHeight = surface.Height,
                dwWidth = surface.Width,
                dwPitchOrLinearSize = GetPitchOrLinearSize(surface.InternalFormat, surface.Width),
                dwDepth = surface.Depth,
                dwMipMapCount = surface.Arrays[0].Mipmaps.Count,
                dwReserved1 = new uint[11],
                ddspf = new DDS_PIXELFORMAT()
                {

                },
                dwCaps = 0,
                dwCaps2 = 0
            };

            if (surface.Arrays.Count > 0 && surface.Arrays[0].Mipmaps.Count > 1)
                Header.dwFlags |= DDSD.MIPMAPCOUNT;

            if (surface.IsCubeMap)
            {
                Header.dwCaps |= DDSCAPS.TEXTURE | DDSCAPS.COMPLEX;
                Header.dwCaps2 |= DDSCAPS2.CUBEMAP_ALLFACES;
            }
            //TODO: format
            Header.ddspf.dwFlags = DDPF.FOURCC;
            Header.ddspf.dwFourCC = 0x30315844;
            if (internalFormatToD3D.ContainsKey(surface.InternalFormat))
                Header.ddspf.dwFourCC = (int)internalFormatToD3D[surface.InternalFormat];

            if (internalFormatToDXGI.ContainsKey(surface.InternalFormat))
            {
                Header.DXT10Header.dxgiFormat = internalFormatToDXGI[surface.InternalFormat];
                Header.DXT10Header.resourceDimension = D3D10_RESOURCE_DIMENSION.D3D10_RESOURCE_DIMENSION_TEXTURE2D;
                Header.DXT10Header.arraySize = (uint)surface.Arrays.Count;
            }

            using (BinaryWriterExt writer = new BinaryWriterExt(new FileStream(fileName, FileMode.Create)))
            {
                Header.Write(writer);
                 
                foreach(var arr in surface.Arrays)
                    foreach (var mip in arr.Mipmaps)
                        writer.Write(mip);
            }
        }

        private static InternalFormat DXGItoInternal(DXGI_FORMAT format)
        {
            foreach (var v in internalFormatToDXGI)
                if (v.Value == format)
                    return v.Key;
            return 0;
        }

        private static readonly Dictionary<InternalFormat, DXGI_FORMAT> internalFormatToDXGI = new Dictionary<InternalFormat, DXGI_FORMAT>()
        {
            { InternalFormat.CompressedRgbaS3tcDxt1Ext, DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM },
            { InternalFormat.CompressedRgbaS3tcDxt3Ext, DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM },
            { InternalFormat.CompressedRgbaS3tcDxt5Ext, DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM },
            { InternalFormat.CompressedRgbaBptcUnorm, DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM },
            { InternalFormat.CompressedSrgbAlphaS3tcDxt1Ext, DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM_SRGB },
            { InternalFormat.CompressedSrgbAlphaS3tcDxt3Ext, DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM_SRGB },
            { InternalFormat.CompressedSrgbAlphaS3tcDxt5Ext, DXGI_FORMAT.DXGI_FORMAT_BC5_SNORM },
            { InternalFormat.CompressedSrgbAlphaBptcUnorm, DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM_SRGB },
            { InternalFormat.CompressedRgbBptcUnsignedFloat, DXGI_FORMAT.DXGI_FORMAT_BC7_TYPELESS },
            { InternalFormat.Rgba32f, DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_FLOAT },
            { InternalFormat.Rgba8, DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM },
        };

        private static readonly Dictionary<InternalFormat, FourCC_DXGI> internalFormatToD3D = new Dictionary<InternalFormat, FourCC_DXGI>()
        {
            { InternalFormat.Rgba32f, FourCC_DXGI.D3DFMT_A32B32G32R32F },
        };

        private static readonly Dictionary<InternalFormat, int> internalFormatToFourCC = new Dictionary<InternalFormat, int>()
        {
            { InternalFormat.CompressedRgbaS3tcDxt1Ext, 0x31545844 },
            { InternalFormat.CompressedRgbaS3tcDxt3Ext, 0x33545844 },
            { InternalFormat.CompressedRgbaS3tcDxt5Ext, 0x35545844 },
            { InternalFormat.CompressedRgbaBptcUnorm, 0x30315844 },
        };
        

        private static int GetPitchOrLinearSize(InternalFormat format, int width)
        {
            if (format.ToString().Contains("Compressed"))
            {
                return Math.Max(1, ((width + 3) / 4)) * (int)TextureFormatInfo.GetBPP(format);
            }
            return (width * (int)TextureFormatInfo.GetBPP(format) + 7) / 8;
        }

        private class DDS_Header
        {
            public uint dwSize = 0x7C;
            public DDSD dwFlags;
            public int dwHeight;
            public int dwWidth;
            public int dwPitchOrLinearSize;
            public int dwDepth;
            public int dwMipMapCount;
            public uint[] dwReserved1 = new uint[11];
            public DDS_PIXELFORMAT ddspf = new DDS_PIXELFORMAT();
            public DDSCAPS dwCaps;
            public DDSCAPS2 dwCaps2;
            public uint dwCaps3;
            public uint dwCaps4;
            public uint dwReserved2;

            public DDS_HEADER_DXT10 DXT10Header = new DDS_HEADER_DXT10();

            public void Read(BinaryReaderExt reader)
            {
                reader.ReadChars(4);
                dwSize = reader.ReadUInt32();
                dwFlags = (DDSD)reader.ReadUInt32();
                dwHeight = reader.ReadInt32();
                dwWidth = reader.ReadInt32();
                dwPitchOrLinearSize = reader.ReadInt32();
                dwDepth = reader.ReadInt32();
                dwMipMapCount = reader.ReadInt32();
                dwReserved1 = new uint[11];
                for(int i = 0; i < 11; i++)
                    dwReserved1[i] = reader.ReadUInt32();
                ddspf.Read(reader);
                dwCaps = (DDSCAPS)reader.ReadInt32();
                dwCaps2 = (DDSCAPS2)reader.ReadInt32();
                dwCaps3 = reader.ReadUInt32();
                dwCaps4 = reader.ReadUInt32();
                dwReserved2 = reader.ReadUInt32();

                if(ddspf.dwFlags.HasFlag(DDPF.FOURCC) && ddspf.dwFourCC == 0x30315844)
                {
                    DXT10Header.Read(reader);
                }
            }

            public void Write(BinaryWriterExt writer)
            {
                writer.Write(new char[] { 'D', 'D', 'S', ' '});
                writer.Write(dwSize);
                writer.Write((int)dwFlags);
                writer.Write(dwHeight);
                writer.Write(dwWidth);
                writer.Write(dwPitchOrLinearSize);
                writer.Write(dwDepth);
                writer.Write(dwMipMapCount);
                foreach (var v in dwReserved1)
                    writer.Write(v);
                ddspf.Write(writer);
                writer.Write((int)dwCaps);
                writer.Write((int)dwCaps2);
                writer.Write(dwCaps3);
                writer.Write(dwCaps4);
                writer.Write(dwReserved2);

                if (ddspf.dwFlags.HasFlag(DDPF.FOURCC) && ddspf.dwFourCC == 0x30315844)
                {
                    DXT10Header.Write(writer);
                }
            }
        }

        public class DDS_PIXELFORMAT
        {
            public uint dwSize = 0x20;
            public DDPF dwFlags;
            public int dwFourCC;
            public uint dwRGBBitCount;
            public uint dwRBitMask;
            public uint dwGBitMask;
            public uint dwBBitMask;
            public uint dwABitMask;

            public void Read(BinaryReaderExt reader)
            {
                dwSize = reader.ReadUInt32();
                dwFlags = (DDPF)reader.ReadInt32();
                dwFourCC = reader.ReadInt32();
                dwRGBBitCount = reader.ReadUInt32();
                dwRBitMask = reader.ReadUInt32();
                dwGBitMask = reader.ReadUInt32();
                dwBBitMask = reader.ReadUInt32();
                dwABitMask = reader.ReadUInt32();
            }

            public void Write(BinaryWriterExt writer)
            {
                writer.Write(dwSize);
                writer.Write((int)dwFlags);
                writer.Write(dwFourCC);
                writer.Write(dwRGBBitCount);
                writer.Write(dwRBitMask);
                writer.Write(dwGBitMask);
                writer.Write(dwBBitMask);
                writer.Write(dwABitMask);
            }
        }

        private class DDS_HEADER_DXT10
        {
            public DXGI_FORMAT dxgiFormat;
            public D3D10_RESOURCE_DIMENSION resourceDimension;
            public uint miscFlag;
            public uint arraySize;
            public uint miscFlags2;

            public void Read(BinaryReaderExt reader)
            {
                dxgiFormat = (DXGI_FORMAT)reader.ReadInt32();
                resourceDimension = (D3D10_RESOURCE_DIMENSION)reader.ReadInt32();
                miscFlag = reader.ReadUInt32();
                arraySize = reader.ReadUInt32();
                miscFlags2 = reader.ReadUInt32();
            }

            public void Write(BinaryWriterExt writer)
            {
                writer.Write((int)dxgiFormat);
                writer.Write((int)resourceDimension);
                writer.Write(miscFlag);
                writer.Write(arraySize);
                writer.Write(miscFlags2);
            }
        }

        public enum D3D10_RESOURCE_DIMENSION
        {
            D3D10_RESOURCE_DIMENSION_UNKNOWN,
            D3D10_RESOURCE_DIMENSION_BUFFER,
            D3D10_RESOURCE_DIMENSION_TEXTURE1D,
            D3D10_RESOURCE_DIMENSION_TEXTURE2D,
            D3D10_RESOURCE_DIMENSION_TEXTURE3D
        };

        public enum FourCC_DXGI
        {
            D3DFMT_A16B16G16R16 = 36,
            D3DFMT_Q16W16V16U16 = 110,
            D3DFMT_R16F = 111,
            D3DFMT_G16R16F = 112,
            D3DFMT_A16B16G16R16F = 113,
            D3DFMT_R32F = 114,
            D3DFMT_G32R32F = 115,
            D3DFMT_A32B32G32R32F = 116,
            D3DFMT_CxV8U8 = 117
        }

        public enum DXGI_FORMAT
        {
            DXGI_FORMAT_UNKNOWN,
            DXGI_FORMAT_R32G32B32A32_TYPELESS,
            DXGI_FORMAT_R32G32B32A32_FLOAT,
            DXGI_FORMAT_R32G32B32A32_UINT,
            DXGI_FORMAT_R32G32B32A32_SINT,
            DXGI_FORMAT_R32G32B32_TYPELESS,
            DXGI_FORMAT_R32G32B32_FLOAT,
            DXGI_FORMAT_R32G32B32_UINT,
            DXGI_FORMAT_R32G32B32_SINT,
            DXGI_FORMAT_R16G16B16A16_TYPELESS,
            DXGI_FORMAT_R16G16B16A16_FLOAT,
            DXGI_FORMAT_R16G16B16A16_UNORM,
            DXGI_FORMAT_R16G16B16A16_UINT,
            DXGI_FORMAT_R16G16B16A16_SNORM,
            DXGI_FORMAT_R16G16B16A16_SINT,
            DXGI_FORMAT_R32G32_TYPELESS,
            DXGI_FORMAT_R32G32_FLOAT,
            DXGI_FORMAT_R32G32_UINT,
            DXGI_FORMAT_R32G32_SINT,
            DXGI_FORMAT_R32G8X24_TYPELESS,
            DXGI_FORMAT_D32_FLOAT_S8X24_UINT,
            DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS,
            DXGI_FORMAT_X32_TYPELESS_G8X24_UINT,
            DXGI_FORMAT_R10G10B10A2_TYPELESS,
            DXGI_FORMAT_R10G10B10A2_UNORM,
            DXGI_FORMAT_R10G10B10A2_UINT,
            DXGI_FORMAT_R11G11B10_FLOAT,
            DXGI_FORMAT_R8G8B8A8_TYPELESS,
            DXGI_FORMAT_R8G8B8A8_UNORM,
            DXGI_FORMAT_R8G8B8A8_UNORM_SRGB,
            DXGI_FORMAT_R8G8B8A8_UINT,
            DXGI_FORMAT_R8G8B8A8_SNORM,
            DXGI_FORMAT_R8G8B8A8_SINT,
            DXGI_FORMAT_R16G16_TYPELESS,
            DXGI_FORMAT_R16G16_FLOAT,
            DXGI_FORMAT_R16G16_UNORM,
            DXGI_FORMAT_R16G16_UINT,
            DXGI_FORMAT_R16G16_SNORM,
            DXGI_FORMAT_R16G16_SINT,
            DXGI_FORMAT_R32_TYPELESS,
            DXGI_FORMAT_D32_FLOAT,
            DXGI_FORMAT_R32_FLOAT,
            DXGI_FORMAT_R32_UINT,
            DXGI_FORMAT_R32_SINT,
            DXGI_FORMAT_R24G8_TYPELESS,
            DXGI_FORMAT_D24_UNORM_S8_UINT,
            DXGI_FORMAT_R24_UNORM_X8_TYPELESS,
            DXGI_FORMAT_X24_TYPELESS_G8_UINT,
            DXGI_FORMAT_R8G8_TYPELESS,
            DXGI_FORMAT_R8G8_UNORM,
            DXGI_FORMAT_R8G8_UINT,
            DXGI_FORMAT_R8G8_SNORM,
            DXGI_FORMAT_R8G8_SINT,
            DXGI_FORMAT_R16_TYPELESS,
            DXGI_FORMAT_R16_FLOAT,
            DXGI_FORMAT_D16_UNORM,
            DXGI_FORMAT_R16_UNORM,
            DXGI_FORMAT_R16_UINT,
            DXGI_FORMAT_R16_SNORM,
            DXGI_FORMAT_R16_SINT,
            DXGI_FORMAT_R8_TYPELESS,
            DXGI_FORMAT_R8_UNORM,
            DXGI_FORMAT_R8_UINT,
            DXGI_FORMAT_R8_SNORM,
            DXGI_FORMAT_R8_SINT,
            DXGI_FORMAT_A8_UNORM,
            DXGI_FORMAT_R1_UNORM,
            DXGI_FORMAT_R9G9B9E5_SHAREDEXP,
            DXGI_FORMAT_R8G8_B8G8_UNORM,
            DXGI_FORMAT_G8R8_G8B8_UNORM,
            DXGI_FORMAT_BC1_TYPELESS,
            DXGI_FORMAT_BC1_UNORM,
            DXGI_FORMAT_BC1_UNORM_SRGB,
            DXGI_FORMAT_BC2_TYPELESS,
            DXGI_FORMAT_BC2_UNORM,
            DXGI_FORMAT_BC2_UNORM_SRGB,
            DXGI_FORMAT_BC3_TYPELESS,
            DXGI_FORMAT_BC3_UNORM,
            DXGI_FORMAT_BC3_UNORM_SRGB,
            DXGI_FORMAT_BC4_TYPELESS,
            DXGI_FORMAT_BC4_UNORM,
            DXGI_FORMAT_BC4_SNORM,
            DXGI_FORMAT_BC5_TYPELESS,
            DXGI_FORMAT_BC5_UNORM,
            DXGI_FORMAT_BC5_SNORM,
            DXGI_FORMAT_B5G6R5_UNORM,
            DXGI_FORMAT_B5G5R5A1_UNORM,
            DXGI_FORMAT_B8G8R8A8_UNORM,
            DXGI_FORMAT_B8G8R8X8_UNORM,
            DXGI_FORMAT_R10G10B10_XR_BIAS_A2_UNORM,
            DXGI_FORMAT_B8G8R8A8_TYPELESS,
            DXGI_FORMAT_B8G8R8A8_UNORM_SRGB,
            DXGI_FORMAT_B8G8R8X8_TYPELESS,
            DXGI_FORMAT_B8G8R8X8_UNORM_SRGB,
            DXGI_FORMAT_BC6H_TYPELESS,
            DXGI_FORMAT_BC6H_UF16,
            DXGI_FORMAT_BC6H_SF16,
            DXGI_FORMAT_BC7_TYPELESS,
            DXGI_FORMAT_BC7_UNORM,
            DXGI_FORMAT_BC7_UNORM_SRGB,
            DXGI_FORMAT_AYUV,
            DXGI_FORMAT_Y410,
            DXGI_FORMAT_Y416,
            DXGI_FORMAT_NV12,
            DXGI_FORMAT_P010,
            DXGI_FORMAT_P016,
            DXGI_FORMAT_420_OPAQUE,
            DXGI_FORMAT_YUY2,
            DXGI_FORMAT_Y210,
            DXGI_FORMAT_Y216,
            DXGI_FORMAT_NV11,
            DXGI_FORMAT_AI44,
            DXGI_FORMAT_IA44,
            DXGI_FORMAT_P8,
            DXGI_FORMAT_A8P8,
            DXGI_FORMAT_B4G4R4A4_UNORM,
            DXGI_FORMAT_P208,
            DXGI_FORMAT_V208,
            DXGI_FORMAT_V408,
            DXGI_FORMAT_FORCE_UINT
        }

        public enum DDSFormat
        {
            RGBA,
            DXT1,
            DXT3,
            DXT5,
            ATI1,
            ATI2
        }

        public enum CubemapFace
        {
            PosX,
            NegX,
            PosY,
            NegY,
            PosZ,
            NegZ
        }

        [Flags]
        public enum DDSD : uint
        {
            CAPS = 0x00000001,
            HEIGHT = 0x00000002,
            WIDTH = 0x00000004,
            PITCH = 0x00000008,
            PIXELFORMAT = 0x00001000,
            MIPMAPCOUNT = 0x00020000,
            LINEARSIZE = 0x00080000,
            DEPTH = 0x00800000
        }

        [Flags]
        public enum DDPF : uint
        {
            ALPHAPIXELS = 0x00000001,
            ALPHA = 0x00000002,
            FOURCC = 0x00000004,
            RGB = 0x00000040,
            YUV = 0x00000200,
            LUMINANCE = 0x00020000,
        }
        [Flags]
        public enum DDSCAPS : uint
        {
            COMPLEX = 0x00000008,
            TEXTURE = 0x00001000,
            MIPMAP = 0x00400000,
        }
        [Flags]
        public enum DDSCAPS2 : uint
        {
            CUBEMAP = 0x00000200,
            CUBEMAP_POSITIVEX = 0x00000400 | CUBEMAP,
            CUBEMAP_NEGATIVEX = 0x00000800 | CUBEMAP,
            CUBEMAP_POSITIVEY = 0x00001000 | CUBEMAP,
            CUBEMAP_NEGATIVEY = 0x00002000 | CUBEMAP,
            CUBEMAP_POSITIVEZ = 0x00004000 | CUBEMAP,
            CUBEMAP_NEGATIVEZ = 0x00008000 | CUBEMAP,
            CUBEMAP_ALLFACES = (CUBEMAP_POSITIVEX | CUBEMAP_NEGATIVEX |
                                  CUBEMAP_POSITIVEY | CUBEMAP_NEGATIVEY |
                                  CUBEMAP_POSITIVEZ | CUBEMAP_NEGATIVEZ),
            VOLUME = 0x00200000
        }
    }
}
