using OpenTK;
using System;

namespace StudioSB.Rendering.Bounding
{
    /// <summary>
    /// Represents a 3d ray 
    /// used for picking in scene
    /// </summary>
    public class Ray
    {
        public Vector3 p1;
        public Vector3 p2;

        public Vector3 Dir { get { return (p1 - p2).Normalized(); } }

        public Ray(Vector3 p1, Vector3 p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="closest"></param>
        /// <returns></returns>
        /// http://csharphelper.com/blog/2016/09/find-the-shortest-distance-between-a-point-and-a-line-segment-in-c/
        public float GetDistanceToSegment(Vector2 pt, Vector2 p1, Vector2 p2, out Vector2 closest)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            if ((dx == 0) && (dy == 0))
            {
                // It's a point not a line segment.
                closest = p1;
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
                return (float)Math.Sqrt(dx * dx + dy * dy);
            }

            // Calculate the t that minimizes the distance.
            float t = ((pt.X - p1.X) * dx + (pt.Y - p1.Y) * dy) /
                (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                closest = new Vector2(p1.X, p1.Y);
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
            }
            else if (t > 1)
            {
                closest = new Vector2(p2.X, p2.Y);
                dx = pt.X - p2.X;
                dy = pt.Y - p2.Y;
            }
            else
            {
                closest = new Vector2(p1.X + t * dx, p1.Y + t * dy);
                dx = pt.X - closest.X;
                dy = pt.Y - closest.Y;
            }

            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Norm"></param>
        /// <returns></returns>
        public Vector3 GetPlaneIntersection(Vector3 Norm, Vector3 Position)
        {
            Vector3 rayP = p1;
            Vector3 rayD = Dir;
            Vector3 planeN = Norm;
            var d = Vector3.Dot(Position, -Norm);
            var t = -(d + rayP.Z * planeN.Z + rayP.Y * planeN.Y + rayP.X * planeN.X) / (rayD.Z * planeN.Z + rayD.Y * planeN.Y + rayD.X * planeN.X);
            return rayP + t * rayD;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sphere"></param>
        /// <param name="rad"></param>
        /// <param name="closest"></param>
        /// <returns></returns>
        public bool CheckSphereHit(Vector3 sphere, float rad, out Vector3 closest)
        {
            Vector3 dirToSphere = sphere - p1;
            Vector3 vLineDir = (p2 - p1).Normalized();
            float fLineLength = 100;

            float t = Vector3.Dot(dirToSphere, vLineDir);

            if (t <= 0.0f)
                closest = p1;
            else if (t >= fLineLength)
                closest = p2;
            else
                closest = p1 + vLineDir * t;

            return (Math.Pow(sphere.X - closest.X, 2)
                + Math.Pow(sphere.Y - closest.Y, 2)
                + Math.Pow(sphere.Z - closest.Z, 2) <= rad * rad);
        }

    }
}
