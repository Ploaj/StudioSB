using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace StudioSB.Rendering.Shapes
{
    /// <summary>
    /// A class for drawing a standard 3d grid floor
    /// </summary>
    public class GridFloor3D
    {
        /// <summary>
        /// Draws a standard 3d line grid floor
        /// </summary>
        public static void Draw(int Size, int Count, Color Color)
        {
            GL.UseProgram(0);
            GL.PushAttrib(AttribMask.AllAttribBits);

            GL.Color3(Color);
            GL.LineWidth(1f);
            GL.Begin(PrimitiveType.Lines);

            //all grid lines
            for (int i = -Count / 2; i < Count / 2; i++)
            {
                for (int j = -Count / 2; j < Count / 2; j++)
                {
                    GL.Vertex3(i * Size, 0, j * Size);
                    GL.Vertex3((i + 1) * Size, 0, j * Size);
                    GL.Vertex3(i * Size, 0, j * Size);
                    GL.Vertex3(i * Size, 0, (j + 1) * Size);
                }
            }
            //Bottom two lines
            if (Count > 0)
            {
                GL.Vertex3(Count / 2 * Size, 0, -Count / 2 * Size);
                GL.Vertex3(Count / 2 * Size, 0, Count / 2 * Size);

                GL.Vertex3(-Count / 2 * Size, 0, Count / 2 * Size);
                GL.Vertex3(Count / 2 * Size, 0, Count / 2 * Size);
            }
            GL.End();

            GL.Disable(EnableCap.DepthTest);

            GL.LineWidth(2f);
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.CornflowerBlue);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, Size, 0);

            GL.Color3(Color.PaleVioletRed);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(Size, 0, 0);

            GL.Color3(Color.LawnGreen);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 0, Size);
            GL.End();

            GL.PopAttrib();
        }

    }
}
