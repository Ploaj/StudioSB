using System.Collections.Generic;
using SFGenericModel;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGenericModel.VertexAttributes;
using SFGraphics.Cameras;

namespace StudioSB.Rendering.Shapes
{
    /// <summary>
    /// A cylinder with hermespherical ends
    /// </summary>
    public class Capsule : GenericMesh<Vector4>
    {
        /// <summary>
        /// static drawing
        /// </summary>
        private static Capsule UnitCapsule;

        public Capsule() : base(GeneratePositions(), PrimitiveType.TriangleStrip)
        {

        }

        /// <summary>
        /// Generates sphere positions and sets half the sphere to be rigged to a separate transform
        /// </summary>
        /// <returns></returns>
        private static List<Vector4> GeneratePositions()
        {
            var sphere = SFShapes.ShapeGenerator.GetSpherePositions(Vector3.Zero, 1, 20).Item1;

            var capsule = new List<Vector4>();

            foreach(var v in sphere)
            {
                Vector4 value = new Vector4();
                value.Xyz = v;
                if (value.Y > 0)
                    value.W = 1;
                capsule.Add(value);
            }

            return capsule;
        }

        public override List<VertexAttribute> GetVertexAttributes()
        {
            return new List<VertexAttribute>()
            {
                new VertexFloatAttribute("Position", ValueCount.Four, VertexAttribPointerType.Float, false),
            };
        }

        /// <summary>
        /// Renders a unit capsule to the given context
        /// </summary>
        public static void DrawCapsule(Camera Camera, float Size, Matrix4 bone1, Matrix4 bone2)
        {
            if(UnitCapsule == null)
                UnitCapsule = new Capsule();

            var shader = ShaderManager.GetShader("Capsule");

            shader.UseProgram();
            
            Matrix4 mvp = Camera.MvpMatrix;
            shader.SetMatrix4x4("mvp", ref mvp);

            shader.SetVector4("Color", 1, 0, 0, 1);

            Vector3 position1 = Vector3.TransformPosition(Vector3.Zero, bone1);
            Vector3 position2 = Vector3.TransformPosition(Vector3.Zero, bone2);

            Vector3 to = position2 - position1;
            to.NormalizeFast();
            Vector3 axis = Vector3.Cross(Vector3.UnitY, to);

            float omega = (float)System.Math.Acos(Vector3.Dot(Vector3.UnitY, to));

            Matrix4 rotation = Matrix4.CreateFromAxisAngle(axis, omega);
            
            Matrix4 transform1 = rotation * Matrix4.CreateTranslation(position1);
            Matrix4 transform2 = rotation * Matrix4.CreateTranslation(position2);

            shader.SetMatrix4x4("transform1", ref transform1);
            shader.SetMatrix4x4("transform2", ref transform2);
            
            shader.SetFloat("Size", Size);
            GL.PointSize(5f);
            UnitCapsule.Draw(shader);
        }
    }
}
