using OpenTK;
using SFGenericModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using SFGenericModel.VertexAttributes;
using SFGraphics.Cameras;

namespace StudioSB.Rendering.Shapes
{
    /// <summary>
    /// A class for rendering generic rectangular prisms
    /// </summary>
    public class RectangularPrism : GenericMesh<Vector4>
    {
        /// <summary>
        /// static drawing
        /// </summary>
        private static RectangularPrism UnitPrism;

        private static List<Vector4> unitPositions = new List<Vector4>()
        {
            //top
            new Vector4(0f, 0f, 0f, 0), // 0 
            new Vector4(1f, 0f, 0f, 0), // 1 
            new Vector4(0f, 1f, 0f, 0), // 2
            new Vector4(0f, 0f, 1f, 0), // 3

            new Vector4(1f, 1f, 0f, 0), // 4
            new Vector4(0f, 1f, 1f, 0), // 5
            new Vector4(1f, 0f, 1f, 0), // 6
            new Vector4(1f, 1f, 1f, 0) // 7
        };

        public static List<int> unitIndicies = new List<int>()
        {
            0, 3, 3, 6, 6, 1, 1, 0,
            5, 7, 7, 4, 4, 2, 2, 5,
            3, 5, 6, 7, 0, 2, 1, 4
        };

        public RectangularPrism() : base(unitPositions, unitIndicies, PrimitiveType.Lines)
        {

        }

        public override List<VertexAttribute> GetVertexAttributes()
        {
            return new List<VertexAttribute>()
            {
                new VertexFloatAttribute("point", ValueCount.Four, VertexAttribPointerType.Float),
            };
        }

        /// <summary>
        /// Renders a unit capsule to the given context
        /// </summary>
        public static void DrawRectangularPrism(Camera Camera, Vector3 Position, Vector3 Size, Matrix4 Transform)
        {
            if (UnitPrism == null)
                UnitPrism = new RectangularPrism();

            var shader = ShaderManager.GetShader("Prism");

            shader.UseProgram();

            Matrix4 mvp = Camera.MvpMatrix;
            shader.SetMatrix4x4("mvp", ref mvp);

            shader.SetVector4("color", 1, 0, 0, 1);
            
            shader.SetMatrix4x4("transform", ref Transform);

            shader.SetVector3("size", Size);
            shader.SetVector3("offset", Position);
            
            GL.LineWidth(5f);
            UnitPrism.Draw(shader, Camera);
        }
    }
}
