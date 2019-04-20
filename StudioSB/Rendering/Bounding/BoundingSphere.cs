using System;
using System.Collections.Generic;
using OpenTK;
using SFGraphics.Cameras;

namespace StudioSB.Rendering.Bounding
{
    public class BoundingSphere
    {
        public float X { get { return XyzRadius.X; } }
        public float Y { get { return XyzRadius.Y; } }
        public float Z { get { return XyzRadius.Z; } }

        public Vector3 Position { get { return XyzRadius.Xyz; } }

        public float Radius { get { return XyzRadius.W; } }

        public Vector4 XyzRadius { get; internal set; }

        public BoundingSphere(float X, float Y, float Z, float Radius)
        {
            XyzRadius = new Vector4(X, Y, Z, Radius);
        }

        public BoundingSphere(IEnumerable<Vector3> points)
        {
            XyzRadius = SFGraphics.Utils.BoundingSphereGenerator.GenerateBoundingSphere(points);
        }

        /// <summary>
        /// Renders sphere in current context
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="transform"></param>
        public void Render(Camera camera, Matrix4 transform)
        {
            Shapes.Sphere.DrawSphereLegacy(Vector3.TransformPosition(Position, transform), Radius, 24, true);
        }
    }
}
