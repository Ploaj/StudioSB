using HSDLib.GX;
using HSDLib.Helpers;
using OpenTK.Graphics.OpenGL;

namespace StudioSB.Scenes.Melee
{
    class GXtoGL
    {
        public static PrimitiveType GLPrimitiveType(GXPrimitiveType gxprim)
        {
            switch (gxprim)
            {
                case GXPrimitiveType.Lines:
                    return PrimitiveType.Lines;
                case GXPrimitiveType.LineStrip:
                    return PrimitiveType.LineStrip;
                case GXPrimitiveType.Points:
                    return PrimitiveType.Points;
                case GXPrimitiveType.Quads:
                    return PrimitiveType.Quads;
                case GXPrimitiveType.TriangleFan:
                    return PrimitiveType.TriangleFan;
                case GXPrimitiveType.Triangles:
                    return PrimitiveType.Triangles;
                case GXPrimitiveType.TriangleStrip:
                    return PrimitiveType.TriangleStrip;
                default:
                    return PrimitiveType.Points;
            }
        }
        public static TextureWrapMode GLWrapMode(GXWrapMode wm)
        {
            switch (wm)
            {
                case GXWrapMode.CLAMP:
                    return TextureWrapMode.Clamp;
                case GXWrapMode.MIRROR:
                    return TextureWrapMode.MirroredRepeat;
                case GXWrapMode.REPEAT:
                    return TextureWrapMode.Repeat;
                default:
                    return TextureWrapMode.Repeat;
            }
        }
    }
}
