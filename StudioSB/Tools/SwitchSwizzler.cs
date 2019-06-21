using OpenTK;
using StudioSB.IO.Formats;
using StudioSB.Scenes;
using System;
using System.Collections.Generic;

namespace StudioSB.Tools
{
    public class SwitchSwizzler
    {
        /**
         * Ported from https://github.com/KillzXGaming/Switch-Toolbox
         */
        public static byte[] GetImageData(SBSurface surface, byte[] ImageData, int ArrayLevel, int MipLevel, int MipCount, int target = 1)
        {
            uint bpp = TextureFormatInfo.GetBPP(surface.InternalFormat);
            uint blkWidth = TextureFormatInfo.GetBlockWidth(surface.InternalFormat);
            uint blkHeight = TextureFormatInfo.GetBlockHeight(surface.InternalFormat);
            uint blkDepth = TextureFormatInfo.GetBlockDepth(surface.InternalFormat);

            uint blockHeight = GetBlockHeight(DivRoundUp((uint)surface.Height, blkHeight));
            uint BlockHeightLog2 = (uint)Convert.ToString(blockHeight, 2).Length - 1;

            uint Pitch = 0;
            uint DataAlignment = 512;
            uint TileMode = 0;

            int linesPerBlockHeight = (1 << (int)BlockHeightLog2) * 8;

            uint ArrayOffset = 0;
            for (int arrayLevel = 0; arrayLevel < surface.ArrayCount; arrayLevel++)
            {
                uint SurfaceSize = 0;
                int blockHeightShift = 0;

                List<uint> MipOffsets = new List<uint>();

                for (int mipLevel = 0; mipLevel < MipCount; mipLevel++)
                {
                    uint width = (uint)Math.Max(1, surface.Width >> mipLevel);
                    uint height = (uint)Math.Max(1, surface.Height >> mipLevel);
                    uint depth = (uint)Math.Max(1, surface.Depth >> mipLevel);

                    uint size = DivRoundUp(width, blkWidth) * DivRoundUp(height, blkHeight) * bpp;

                    if (Pow2RoundUp(DivRoundUp(height, blkWidth)) < linesPerBlockHeight)
                        blockHeightShift += 1;


                    uint width__ = DivRoundUp(width, blkWidth);
                    uint height__ = DivRoundUp(height, blkHeight);

                    //Calculate the mip size instead
                    byte[] AlignedData = new byte[(RoundUp(SurfaceSize, DataAlignment) - SurfaceSize)];
                    SurfaceSize += (uint)AlignedData.Length;
                    MipOffsets.Add(SurfaceSize);

                    //Get the first mip offset and current one and the total image size
                    int msize = (int)((MipOffsets[0] + ImageData.Length - MipOffsets[mipLevel]) / surface.ArrayCount);

                    if (msize > ImageData.Length - (ArrayOffset + MipOffsets[mipLevel]))
                        msize = (int)(ImageData.Length - (ArrayOffset + MipOffsets[mipLevel]));
                    byte[] data_ = new byte[msize];
                    if (ArrayLevel == arrayLevel && MipLevel == mipLevel)
                        Array.Copy(ImageData, ArrayOffset + MipOffsets[mipLevel], data_, 0, msize);
                    try
                    {
                        Pitch = RoundUp(width__ * bpp, 64);
                        SurfaceSize += Pitch * RoundUp(height__, Math.Max(1, blockHeight >> blockHeightShift) * 8);

                        if (ArrayLevel == arrayLevel && MipLevel == mipLevel)
                        {
                            //Console.WriteLine($"{width} {height} {blkWidth} {blkHeight} {target} {bpp} {TileMode} {(int)Math.Max(0, BlockHeightLog2 - blockHeightShift)} {data_.Length}");
                            byte[] result = Deswizzle(width, height, depth, blkWidth, blkHeight, blkDepth, target, bpp, TileMode, (int)Math.Max(0, BlockHeightLog2 - blockHeightShift), data_);
                            //Create a copy and use that to remove uneeded data
                            byte[] result_ = new byte[size];
                            Array.Copy(result, 0, result_, 0, size);
                            result = null;

                            return result_;
                        }
                    }
                    catch (Exception e)
                    {
                        System.Windows.Forms.MessageBox.Show($"Failed to swizzle texture {surface.Name}!");
                        Console.WriteLine(e);

                        return new byte[0];
                    }
                }

                ArrayOffset += (uint)(ImageData.Length / surface.ArrayCount);
            }
            return new byte[0];
        }

        public static byte[] CreateBuffer(SBSurface surface, int target = 1)
        {
            List<byte> ImageData = new List<byte>();

            if (surface.Arrays.Count == 0)
                return ImageData.ToArray();

            var MipCount = surface.Arrays[0].Mipmaps.Count;
            uint bpp = TextureFormatInfo.GetBPP(surface.InternalFormat);
            uint blkWidth = TextureFormatInfo.GetBlockWidth(surface.InternalFormat);
            uint blkHeight = TextureFormatInfo.GetBlockHeight(surface.InternalFormat);
            uint blkDepth = TextureFormatInfo.GetBlockDepth(surface.InternalFormat);

            uint blockHeight = GetBlockHeight(DivRoundUp((uint)surface.Height, blkHeight));
            uint BlockHeightLog2 = (uint)Convert.ToString(blockHeight, 2).Length - 1;

            uint Pitch = 0;
            uint DataAlignment = 512;
            uint TileMode = 0;

            int linesPerBlockHeight = (1 << (int)BlockHeightLog2) * 8;

            //uint ArrayOffset = 0;
            for (int arrayLevel = 0; arrayLevel < surface.Arrays.Count; arrayLevel++)
            {
                uint SurfaceSize = 0;
                int blockHeightShift = 0;

                List<uint> MipOffsets = new List<uint>();

                for (int mipLevel = 0; mipLevel < MipCount; mipLevel++)
                {
                    uint width = (uint)Math.Max(1, surface.Width >> mipLevel);
                    uint height = (uint)Math.Max(1, surface.Height >> mipLevel);
                    uint depth = (uint)Math.Max(1, surface.Depth >> mipLevel);

                    uint size = DivRoundUp(width, blkWidth) * DivRoundUp(height, blkHeight) * bpp;

                    if (Pow2RoundUp(DivRoundUp(height, blkWidth)) < linesPerBlockHeight)
                        blockHeightShift += 1;


                    uint width__ = DivRoundUp(width, blkWidth);
                    uint height__ = DivRoundUp(height, blkHeight);

                    //Calculate the mip size instead
                    byte[] AlignedData = new byte[(RoundUp(SurfaceSize, DataAlignment) - SurfaceSize)];
                    SurfaceSize += (uint)AlignedData.Length;
                    MipOffsets.Add(SurfaceSize);

                    //Get the first mip offset and current one and the total image size
                    int msize = (int)((MipOffsets[0] + surface.Arrays[arrayLevel].Mipmaps[mipLevel].Length - MipOffsets[mipLevel]) / surface.ArrayCount);
                    
                    //try
                    {
                        Pitch = RoundUp(width__ * bpp, 64);
                        SurfaceSize += Pitch * RoundUp(height__, Math.Max(1, blockHeight >> blockHeightShift) * 8);

                        //Console.WriteLine($"{width} {height} {blkWidth} {blkHeight} {target} {bpp} {TileMode} {(int)Math.Max(0, BlockHeightLog2 - blockHeightShift)}");
                        var mipData = surface.Arrays[arrayLevel].Mipmaps[mipLevel];
                        /*byte[] padded = new byte[mipData.Length * 2];
                        Array.Copy(mipData, 0, padded, 0, mipData.Length);
                        mipData = padded;*/
                        byte[] result = Swizzle(width, height, depth, blkWidth, blkHeight, blkDepth, target, bpp, TileMode, (int)Math.Max(0, BlockHeightLog2 - blockHeightShift), mipData);
                        //Console.WriteLine(result.Length + " " + surface.Mipmaps[mipLevel].Length);
                        ImageData.AddRange(result);
                    }
                    /*catch (Exception e)
                    {
                        System.Windows.Forms.MessageBox.Show($"Failed to swizzle texture {surface.Name}!");
                        Console.WriteLine(e);

                        return new byte[0];
                    }*/
                }

                // alignment
                if(arrayLevel != surface.Arrays.Count - 1)
                ImageData.AddRange(new byte[0x1000 - (ImageData.Count % 0x1000)]);
            }
            return ImageData.ToArray();
        }

        public static readonly Dictionary<NUTEX_FORMAT, Vector2> BlockDiminsions = new Dictionary<NUTEX_FORMAT, Vector2>()
        {
            { NUTEX_FORMAT.B8G8R8A8_UNORM, new Vector2(1, 1) },
            { NUTEX_FORMAT.B8G8R8A8_SRGB, new Vector2(1, 1) },
            { NUTEX_FORMAT.R8G8B8A8_UNORM, new Vector2(1, 1) },
            { NUTEX_FORMAT.R8G8B8A8_SRGB, new Vector2(1, 1) },
            { NUTEX_FORMAT.R32G32B32A32_FLOAT, new Vector2(1, 1) },
            { NUTEX_FORMAT.BC1_UNORM, new Vector2(4, 4) },
            { NUTEX_FORMAT.BC1_SRGB, new Vector2(4, 4) },
            { NUTEX_FORMAT.BC2_UNORM, new Vector2(4, 4) },
            { NUTEX_FORMAT.BC2_SRGB, new Vector2(4, 4) },
            { NUTEX_FORMAT.BC3_UNORM, new Vector2(4, 4) },
            { NUTEX_FORMAT.BC3_SRGB, new Vector2(4, 4) },
            { NUTEX_FORMAT.BC4_UNORM, new Vector2(1, 1) },
            { NUTEX_FORMAT.BC4_SNORM, new Vector2(1, 1) },
            { NUTEX_FORMAT.BC5_UNORM, new Vector2(1, 1) },
            { NUTEX_FORMAT.BC5_SNORM, new Vector2(1, 1) },
            { NUTEX_FORMAT.BC6_UFLOAT, new Vector2(4, 4) },
            { NUTEX_FORMAT.BC7_SRGB, new Vector2(4, 4) },
            { NUTEX_FORMAT.BC7_UNORM, new Vector2(4, 4) },
        };

        public static uint GetBpps(NUTEX_FORMAT format)
        {
            switch (format)
            {
                case NUTEX_FORMAT.R8G8B8A8_UNORM:
                case NUTEX_FORMAT.R8G8B8A8_SRGB:
                case NUTEX_FORMAT.B8G8R8A8_UNORM:
                    return 4;
                case NUTEX_FORMAT.BC1_UNORM:
                    return 8;
                case NUTEX_FORMAT.BC1_SRGB:
                    return 8;
                case NUTEX_FORMAT.BC4_UNORM:
                    return 8;
                case NUTEX_FORMAT.BC4_SNORM:
                    return 8;
                case NUTEX_FORMAT.R32G32B32A32_FLOAT:
                case NUTEX_FORMAT.BC2_UNORM:
                    return 8;
                case NUTEX_FORMAT.BC2_SRGB:
                    return 8;
                case NUTEX_FORMAT.BC3_UNORM:
                    return 16;
                case NUTEX_FORMAT.BC3_SRGB:
                    return 16;
                case NUTEX_FORMAT.BC5_UNORM:
                case NUTEX_FORMAT.BC5_SNORM:
                case NUTEX_FORMAT.BC6_UFLOAT:
                case NUTEX_FORMAT.BC7_UNORM:
                case NUTEX_FORMAT.BC7_SRGB:
                    return 16;
                default:
                    return 0;
            }
        }

        /*---------------------------------------
         * 
         * Code ported from AboodXD's BNTX Extractor https://github.com/aboood40091/BNTX-Extractor/blob/master/swizzle.py
         * 
         *---------------------------------------*/

        public static uint GetBlockHeight(uint height)
        {
            uint blockHeight = Pow2RoundUp(height / 8);
            if (blockHeight > 16)
                blockHeight = 16;

            return blockHeight;
        }

        public static uint DivRoundUp(uint n, uint d)
        {
            return (n + d - 1) / d;
        }

        public static uint RoundUp(uint x, uint y)
        {
            return ((x - 1) | (y - 1)) + 1;
        }

        public static uint Pow2RoundUp(uint x)
        {
            x -= 1;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            return x + 1;
        }

        public static byte[] Swizzle(uint width, uint height, uint depth, uint blkWidth, uint blkHeight, uint blkDepth, int roundPitch, uint bpp, uint tileMode, int blockHeightLog2, byte[] data, int toSwizzle)
        {
            uint block_height = (uint)(1 << blockHeightLog2);

            width = DivRoundUp(width, blkWidth);
            height = DivRoundUp(height, blkHeight);

            uint pitch;
            uint surfSize;
            if (tileMode == 1)
            {
                pitch = width * bpp;

                if (roundPitch == 1)
                    pitch = RoundUp(pitch, 32);

                surfSize = pitch * height;
            }
            else
            {
                pitch = RoundUp(width * bpp, 64);
                surfSize = pitch * RoundUp(height, block_height * 8);
            }

            byte[] result = new byte[surfSize];

            for (uint y = 0; y < height; y++)
            {
                for (uint x = 0; x < width; x++)
                {
                    uint pos;
                    uint pos_;

                    if (tileMode == 1)
                        pos = y * pitch + x * bpp;
                    else
                        pos = GetAddrBlockLinear(x, y, width, bpp, 0, block_height);

                    pos_ = (y * width + x) * bpp;

                    if (pos + bpp <= surfSize)
                    {
                        if (toSwizzle == 0)
                            Array.Copy(data, pos, result, pos_, bpp);
                        else
                        {
                            if(pos_ < data.Length)
                                Array.Copy(data, pos_, result, pos, bpp);
                        }
                    }
                }
            }
            return result;
        }

        public static byte[] Deswizzle(uint width, uint height, uint depth, uint blkWidth, uint blkHeight, uint blkDepth, int roundPitch, uint bpp, uint tileMode, int size_range, byte[] data)
        {
            return Swizzle(width, height, depth, blkWidth, blkHeight, blkDepth, roundPitch, bpp, tileMode, size_range, data, 0);
        }

        public static byte[] Swizzle(uint width, uint height, uint depth, uint blkWidth, uint blkHeight, uint blkDepth, int roundPitch, uint bpp, uint tileMode, int size_range, byte[] data)
        {
            return Swizzle(width, height, depth, blkWidth, blkHeight, blkDepth, roundPitch, bpp, tileMode, size_range, data, 1);
        }

        static uint GetAddrBlockLinear(uint x, uint y, uint width, uint bytes_per_pixel, uint base_address, uint block_height)
        {
            /*
              From Tega X1 TRM 
                               */
            uint image_width_in_gobs = DivRoundUp(width * bytes_per_pixel, 64);


            uint GOB_address = (base_address
                                + (y / (8 * block_height)) * 512 * block_height * image_width_in_gobs
                                + (x * bytes_per_pixel / 64) * 512 * block_height
                                + (y % (8 * block_height) / 8) * 512);

            x *= bytes_per_pixel;

            uint Address = (GOB_address + ((x % 64) / 32) * 256 + ((y % 8) / 2) * 64
                            + ((x % 32) / 16) * 32 + (y % 2) * 16 + (x % 16));
            return Address;
        }
    }
}
