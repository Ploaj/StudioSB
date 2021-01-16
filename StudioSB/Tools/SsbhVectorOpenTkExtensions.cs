namespace StudioSB.Tools
{
    public static class SsbhVectorOpenTkExtensions
    {
        public static OpenTK.Vector3 ToOpenTK(this SSBHLib.Formats.Vector3 vector3)
        {
            return new OpenTK.Vector3(vector3.X, vector3.Y, vector3.Z);
        }

        public static SSBHLib.Formats.Vector3 ToSsbh(this OpenTK.Vector3 vector3)
        {
            return new SSBHLib.Formats.Vector3(vector3.X, vector3.Y, vector3.Z);
        }

        public static OpenTK.Vector4 ToOpenTK(this SSBHLib.Formats.Vector4 vector4)
        {
            return new OpenTK.Vector4(vector4.X, vector4.Y, vector4.Z, vector4.W);
        }

        public static SSBHLib.Formats.Vector4 ToSsbh(this OpenTK.Vector4 vector4)
        {
            return new SSBHLib.Formats.Vector4(vector4.X, vector4.Y, vector4.Z, vector4.W);
        }

        public static OpenTK.Matrix3 ToOpenTK(this SSBHLib.Formats.Matrix3x3 matrix)
        {
            return new OpenTK.Matrix3(matrix.Row1.ToOpenTK(), matrix.Row2.ToOpenTK(), matrix.Row3.ToOpenTK());
        }

        public static SSBHLib.Formats.Matrix3x3 ToSsbh(this OpenTK.Matrix3 matrix)
        {
            return new SSBHLib.Formats.Matrix3x3(matrix.Row0.ToSsbh(), matrix.Row1.ToSsbh(), matrix.Row2.ToSsbh());
        }

        public static OpenTK.Matrix4 ToOpenTK(this SSBHLib.Formats.Matrix4x4 matrix)
        {
            return new OpenTK.Matrix4(matrix.Row1.X, matrix.Row1.Y, matrix.Row1.Z, matrix.Row1.W,
                matrix.Row2.X, matrix.Row2.Y, matrix.Row2.Z, matrix.Row2.W,
                matrix.Row3.X, matrix.Row3.Y, matrix.Row3.Z, matrix.Row3.W,
                matrix.Row4.X, matrix.Row4.Y, matrix.Row4.Z, matrix.Row4.W);
        }

        public static SSBHLib.Formats.Matrix4x4 ToSsbh(this OpenTK.Matrix4 matrix)
        {
            return new SSBHLib.Formats.Matrix4x4(matrix.Row0.ToSsbh(), matrix.Row1.ToSsbh(), matrix.Row2.ToSsbh(), matrix.Row3.ToSsbh());
        }
    }
}
