using OpenTK;
using System;

namespace StudioSB.Rendering.Bounding
{
    public class Ray
    {
        private Vector3 p1;
        private Vector3 p2;

        public Ray(Vector3 p1, Vector3 p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }

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
