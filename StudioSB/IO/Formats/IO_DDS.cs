using StudioSB.Scenes;
using StudioSB.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioSB.IO.Formats
{
    public class IO_DDS
    {
        public void Export(SBSurface surface)
        {
            DDS_Header header = new DDS_Header()
            {
                dwFlags = (DDSD.CAPS | DDSD.HEIGHT | DDSD.WIDTH | DDSD.PIXELFORMAT | DDSD.MIPMAPCOUNT | DDSD.LINEARSIZE),
                dwHeight = surface.Height,
                dwWidth = surface.Width,
                dwPitchOrLinearSize = GetPitchOrLinearSize(),
                dwDepth = surface.Depth,
                dwMipMapCount = surface.Mipmaps.Count,
                dwReserved1 = new uint[11],
                ddspf = new DDS_PIXELFORMAT()
                {

                },
                dwCaps = DDSCAPS.TEXTURE,
                dwCaps2 = 0
            };
            header.ddspf.dwFlags = DDPF.FOURCC;
            header.ddspf.dwFourCC = 0x30315844;


        }

        public static SBSurface Import(string FileName)
        {
            SBSurface surface = new SBSurface();

            using (BinaryReaderExt reader = new BinaryReaderExt(new FileStream(FileName, FileMode.Open)))
            {
                DDS_Header header = new DDS_Header();
                header.Read(reader);
                surface.Width = header.dwWidth;
                surface.Height = header.dwHeight;
                surface.Depth = header.dwDepth;

                surface.InternalFormat = OpenTK.Graphics.OpenGL.InternalFormat.CompressedRgbaBptcUnorm;

                // TODO: read other mips
                surface.Name = Path.GetFileNameWithoutExtension(FileName);
                int w = surface.Width;
                int h = surface.Height;

                for(int i = 0; i < (header.dwFlags.HasFlag(DDSD.MIPMAPCOUNT) ? header.dwMipMapCount : 1); i++)
                {
                    surface.Mipmaps.Add(reader.ReadBytes(w * h * 2));
                    w /= 2;
                    h /= 2;
                }
                reader.PrintPosition();
            }

            return surface;
        }

        private static int GetPitchOrLinearSize()
        {
            return 0;
            //TODO:
            //max(1, ((width + 3) / 4)) * block - size;
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
