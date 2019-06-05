using OpenTK;
using OpenTK.Graphics.OpenGL;
using StudioSB.Scenes;
using StudioSB.Tools;
using System;
using System.Collections.Generic;
using System.IO;

namespace StudioSB.IO.Formats
{
    public class IO_NUTEXB
    {
        
        public static SBSurface Open(string FilePath)
        {
            using (BinaryReader reader = new BinaryReader(new FileStream(FilePath, FileMode.Open)))
            {
                // TODO: Why are there empty streams?
                if (reader.BaseStream.Length == 0)
                    return null;

                SBSurface surface = new SBSurface();

                reader.BaseStream.Position = reader.BaseStream.Length - 0xB0;

                int[] mipmapSizes = new int[16];
                for (int i = 0; i < mipmapSizes.Length; i++)
                    mipmapSizes[i] = reader.ReadInt32();

                reader.ReadChars(4); // TNX magic

                string texName = ReadTexName(reader);
                surface.Name = texName.ToLower();

                surface.Width = reader.ReadInt32();
                surface.Height = reader.ReadInt32();
                surface.Depth = reader.ReadInt32();

                var Format = (NUTEX_FORMAT)reader.ReadByte();

                reader.ReadByte();

                ushort Padding = reader.ReadUInt16();
                reader.ReadUInt32();

                int MipCount = reader.ReadInt32();
                int Alignment = reader.ReadInt32();
                int ArrayCount = reader.ReadInt32();
                int ImageSize = reader.ReadInt32();
                char[] Magic = reader.ReadChars(4);
                int MajorVersion = reader.ReadInt16();
                int MinorVersion = reader.ReadInt16();
                

                if (pixelFormatByNuTexFormat.ContainsKey(Format))
                    surface.PixelFormat = pixelFormatByNuTexFormat[Format];

                if (internalFormatByNuTexFormat.ContainsKey(Format))
                    surface.InternalFormat = internalFormatByNuTexFormat[Format];

                reader.BaseStream.Position = 0;
                byte[] ImageData = reader.ReadBytes(ImageSize);

                for (int i = 0; i < MipCount; i++) // 
                {
                    byte[] deswiz = Tools.SwitchSwizzler.GetImageData(surface, ImageData, 0, i, MipCount);
                    surface.Mipmaps.Add(deswiz);
                }
                
                return surface;
            }
        }

        public static void Export(string FileName, SBSurface surface)
        {
            using (BinaryWriter writer = new BinaryWriter(new FileStream(FileName, FileMode.Create)))
            {
                List<byte> mipData = new List<byte>();
                foreach(var mip in surface.Mipmaps)
                {
                    mipData.AddRange(mip);
                }
                writer.Write(SwitchSwizzler.CreateImageData(surface));

                uint ImageSize = (uint)writer.BaseStream.Position;

                foreach (var mip in surface.Mipmaps)
                {
                    writer.Write(mip.Length);
                }
                for (int i = surface.Mipmaps.Count; i < 0x10; i++)
                    writer.Write(0);

                writer.Write(new char[] { ' ', 'X', 'N', 'T'});
                writer.Write(surface.Name.ToCharArray());
                writer.Write(new byte[0x40 - surface.Name.Length]);
                writer.Write(surface.Width);
                writer.Write(surface.Height);
                writer.Write(surface.Depth);
                writer.Write((byte)TexFormatByInternalFormat(surface.InternalFormat)); // format
                writer.Write((byte)4); // unknown usually 4
                writer.Write((short)0); // pad
                writer.Write(4); // unknown usually 4
                writer.Write(surface.Mipmaps.Count);
                writer.Write(0x1000); // alignment
                writer.Write(1); // array count
                writer.Write(ImageSize);

                writer.Write(new char[] { ' ', 'X', 'E', 'T' });
                writer.Write((short)1); // version major
                writer.Write((short)2); // version minor
            }
        }

        private static string ReadTexName(BinaryReader reader)
        {
            var result = "";
            for (int i = 0; i < 0x40; i++)
            {
                byte b = reader.ReadByte();
                if (b != 0)
                    result += (char)b;
            }

            return result;
        }

        private static NUTEX_FORMAT TexFormatByInternalFormat(InternalFormat format)
        {
            foreach (var v in internalFormatByNuTexFormat)
                if (v.Value == format)
                    return v.Key;
            return NUTEX_FORMAT.BC1_SRGB;
        }

        public static readonly Dictionary<NUTEX_FORMAT, InternalFormat> internalFormatByNuTexFormat = new Dictionary<NUTEX_FORMAT, InternalFormat>()
        {
            { NUTEX_FORMAT.R8G8B8A8_SRGB, InternalFormat.SrgbAlpha },
            { NUTEX_FORMAT.R8G8B8A8_UNORM, InternalFormat.Rgba },
            { NUTEX_FORMAT.R32G32B32A32_FLOAT, InternalFormat.Rgba },
            { NUTEX_FORMAT.B8G8R8A8_UNORM, InternalFormat.Rgba },
            { NUTEX_FORMAT.B8G8R8A8_SRGB, InternalFormat.Srgb },
            { NUTEX_FORMAT.BC1_UNORM, InternalFormat.CompressedRgbaS3tcDxt1Ext },
            { NUTEX_FORMAT.BC1_SRGB, InternalFormat.CompressedSrgbAlphaS3tcDxt1Ext },
            { NUTEX_FORMAT.BC2_UNORM, InternalFormat.CompressedRgbaS3tcDxt3Ext },
            { NUTEX_FORMAT.BC2_SRGB, InternalFormat.CompressedSrgbAlphaS3tcDxt3Ext },
            { NUTEX_FORMAT.BC3_UNORM, InternalFormat.CompressedRgbaS3tcDxt5Ext },
            { NUTEX_FORMAT.BC3_SRGB, InternalFormat.CompressedSrgbAlphaS3tcDxt5Ext },
            { NUTEX_FORMAT.BC4_UNORM, InternalFormat.Rgba },
            { NUTEX_FORMAT.BC4_SNORM, InternalFormat.Rgba },
            { NUTEX_FORMAT.BC5_UNORM, InternalFormat.Rgba },
            { NUTEX_FORMAT.BC5_SNORM, InternalFormat.Rgba },
            { NUTEX_FORMAT.BC6_UFLOAT, InternalFormat.CompressedRgbBptcUnsignedFloat },
            { NUTEX_FORMAT.BC7_UNORM, InternalFormat.CompressedRgbaBptcUnorm },
            { NUTEX_FORMAT.BC7_SRGB, InternalFormat.CompressedSrgbAlphaBptcUnorm }
        };


        /// <summary>
        /// Channel information for uncompressed formats.
        /// </summary>
        public static readonly Dictionary<NUTEX_FORMAT, PixelFormat> pixelFormatByNuTexFormat = new Dictionary<NUTEX_FORMAT, PixelFormat>()
        {
            { NUTEX_FORMAT.R8G8B8A8_SRGB, PixelFormat.Rgba },
            { NUTEX_FORMAT.R8G8B8A8_UNORM, PixelFormat.Rgba },
            { NUTEX_FORMAT.B8G8R8A8_UNORM, PixelFormat.Bgra },
            { NUTEX_FORMAT.B8G8R8A8_SRGB, PixelFormat.Bgra },
        };
    }

    public enum NUTEX_FORMAT
    {
        R8G8B8A8_UNORM = 0,
        R8G8B8A8_SRGB = 0x05,
        R32G32B32A32_FLOAT = 0x34,
        B8G8R8A8_UNORM = 0x50,
        B8G8R8A8_SRGB = 0x55,
        BC1_UNORM = 0x80,
        BC1_SRGB = 0x85,
        BC2_UNORM = 0x90,
        BC2_SRGB = 0x95,
        BC3_UNORM = 0xa0,
        BC3_SRGB = 0xa5,
        BC4_UNORM = 0xb0,
        BC4_SNORM = 0xb5,
        BC5_UNORM = 0xc0,
        BC5_SNORM = 0xc5,
        BC6_UFLOAT = 0xd7,
        BC7_UNORM = 0xe0,
        BC7_SRGB = 0xe5
    }
}
