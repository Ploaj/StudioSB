using System;
using System.Collections.Generic;
using OpenTK;
using SFGraphics.Cameras;

namespace StudioSB.Rendering.Bounding
{
    /// <summary>
    /// Represents an axis aligned bounding box
    /// </summary>
    public class AABoundingBox
    {
        public Vector3 Min { get; internal set; }
        public Vector3 Max { get; internal set; }

        public Vector3 Center {
            get
            {
                return (Max + Min) / 2;
            }
        }

        public Vector3 Size
        {
            get
            {
                return Max - Min;
            }
        }

        public AABoundingBox(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }

        public AABoundingBox(IEnumerable<Vector3> points)
        {
            var bb = GenerateFromPoints(points);
            Min = bb.Min;
            Max = bb.Max;
        }

        /// <summary>
        /// Generates a very simple Axis Aligned Bounding Box
        /// </summary>
        /// <param name="points"></param>
        public static AABoundingBox GenerateFromPoints(IEnumerable<Vector3> points)
        {
            var max = new Vector3(-float.MaxValue, -float.MaxValue, -float.MaxValue);
            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

            foreach (Vector3 p in points)
            {
                max.X = Math.Max(max.X, p.X);
                max.Y = Math.Max(max.Y, p.Y);
                max.Z = Math.Max(max.Z, p.Z);
                min.X = Math.Min(min.X, p.X);
                min.Y = Math.Min(min.Y, p.Y);
                min.Z = Math.Min(min.Z, p.Z);
            }

            return new AABoundingBox(min, max);
        }

        /// <summary>
        /// Renders box in current context
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="parentTransform"></param>
        public void Render(Camera camera, Matrix4 transform)
        {
            Shapes.RectangularPrism.DrawRectangularPrism(camera, Vector3.Zero, Size, Matrix4.CreateTranslation(Center) * transform);
        }
    }
}
