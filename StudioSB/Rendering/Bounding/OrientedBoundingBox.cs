using System.Collections.Generic;
using OpenTK;
using SFGraphics.Cameras;

namespace StudioSB.Rendering.Bounding
{
    // oriented code example: https://github.com/juj/MathGeoLib/blob/master/src/Geometry/OBB.cpp
    /// <summary>
    /// Oriented bounding box
    /// Not optimal
    /// </summary>
    public class OrientedBoundingBox
    {
        public Vector3 Position { get; internal set; }
        public Vector3 Size { get; internal set; }
        public Matrix3 Transform { get; internal set; }

        public OrientedBoundingBox(Vector3 position, Vector3 size, Matrix3 transform)
        {
            Position = position;
            Size = size;
            Transform = transform;
        }

        public OrientedBoundingBox(IEnumerable<Vector3> points)
        {
            //TODO: generate optimal box
            // current is just using AABB
            var AABB = AABoundingBox.GenerateFromPoints(points);
            Position = AABB.Center;
            Size = AABB.Size / 2;
            Transform = Matrix3.Identity;
        }
        
        /// <summary>
        /// Renders box in current context
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="parentTransform"></param>
        public void Render(Camera camera, Matrix4 transform)
        {
            Shapes.RectangularPrism.DrawRectangularPrism(camera, Vector3.Zero, Size * 2, new Matrix4(Transform) * Matrix4.CreateTranslation(Position) * transform);
        }
    }
}
