using System;
using System.Collections.Generic;
using OpenTK;

namespace StudioSB.Tools
{
    /// <summary>
    /// oriented code example: https://github.com/juj/MathGeoLib/blob/master/src/Geometry/OBB.cpp
    /// </summary>
    public class BBGenerator
    {

        /// <summary>
        /// Generates a very simple Axis Aligned Bounding Box
        /// </summary>
        /// <param name="points"></param>
        public static void GenerateAABB(List<Vector3> points, out Vector3 max, out Vector3 min)
        {
            max = new Vector3(-float.MaxValue, -float.MaxValue, -float.MaxValue);
            min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

            foreach(Vector3 p in points)
            {
                max.X = Math.Max(max.X, p.X);
                max.Y = Math.Max(max.Y, p.Y);
                max.Z = Math.Max(max.Z, p.Z);
                min.X = Math.Min(min.X, p.X);
                min.Y = Math.Min(min.Y, p.Y);
                min.Z = Math.Min(min.Z, p.Z);
            }
        }

    }
}
