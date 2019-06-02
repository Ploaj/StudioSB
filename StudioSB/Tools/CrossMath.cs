using System;
using OpenTK;

namespace StudioSB.Tools
{
    public class CrossMath
    {
        /// <summary>
        /// Clamps value between a minimum and maximum value
        /// </summary>
        /// <param name="v"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float Clamp(float v, float min, float max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }

        /// <summary>
        /// Converts Euler angles to a quaternion
        /// </summary>
        /// <param name="EulerAngles"></param>
        /// <returns></returns>
        public static Quaternion ToQuaternion(Vector3 EulerAngles)
        {
            // Abbreviations for the various angular functions
            float yaw = EulerAngles.Z;
            float pitch = EulerAngles.Y;
            float roll = EulerAngles.X;

            double cy = Math.Cos(yaw * 0.5);
            double sy = Math.Sin(yaw * 0.5);
            double cp = Math.Cos(pitch * 0.5);
            double sp = Math.Sin(pitch * 0.5);
            double cr = Math.Cos(roll * 0.5);
            double sr = Math.Sin(roll * 0.5);

            Quaternion q = new Quaternion();
            q.W = (float)(cy * cp * cr + sy * sp * sr);
            q.X = (float)(cy * cp * sr - sy * sp * cr);
            q.Y = (float)(sy * cp * sr + cy * sp * cr);
            q.Z = (float)(sy * cp * cr - cy * sp * sr);
            return q;
        }

        /// <summary>
        /// Converts Euler angles into a Quaternion
        /// </summary>
        /// <param name="RX"></param>
        /// <param name="RY"></param>
        /// <param name="RZ"></param>
        /// <returns></returns>
        public static Quaternion ToQuaternion(float RX, float RY, float RZ)
        {
            return ToQuaternion(new Vector3(RX, RY, RZ));
        }

        /// <summary>
        /// Converts quaternion into euler angles in XYZ order
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public static Vector3 ToEulerAnglesXYZ(Quaternion q)
        {
            Matrix4 mat = Matrix4.CreateFromQuaternion(q);
            float x, y, z;
            y = (float)Math.Asin(Clamp(mat.M13, -1, 1));

            if (Math.Abs(mat.M13) < 0.99999)
            {
                x = (float)Math.Atan2(-mat.M23, mat.M33);
                z = (float)Math.Atan2(-mat.M12, mat.M11);
            }
            else
            {
                x = (float)Math.Atan2(mat.M32, mat.M22);
                z = 0;
            }
            return new Vector3(x, y, z) * -1;
        }

        /// <summary>
        /// Converts quaternion into euler angles in ZYX order
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public static Vector3 ToEulerAngles(Quaternion q)
        {
            Matrix4 mat = Matrix4.CreateFromQuaternion(q);
            float x, y, z;

            y = (float)Math.Asin(-Clamp(mat.M31, -1, 1));

            if (Math.Abs(mat.M31) < 0.99999)
            {
                x = (float)Math.Atan2(mat.M32, mat.M33);
                z = (float)Math.Atan2(mat.M21, mat.M11);
            }
            else
            {
                x = 0;
                z = (float)Math.Atan2(-mat.M12, mat.M22);
            }
            return new Vector3(x, y, z);
        }

        public static bool FastDistance(Vector3 p1, Vector3 p2, float distance)
        {
            return Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2) + Math.Pow(p1.Z - p2.Z, 2) < distance * distance;
        }
    }
}
