using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace StudioSB.Rendering.Shapes
{
    public class Spawn
    {
        private static Vector3[] Positions = new Vector3[]
        {
            // Top
            new Vector3(-1, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(0, 0, -1),
            
            new Vector3(0, 0, -1),
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 0),

            // Sides
            new Vector3(-1, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(0, -1, 0),

            new Vector3(0, 0, 1),
            new Vector3(1, 0, 0),
            new Vector3(0, -1, 0),

            new Vector3(1, 0, 0),
            new Vector3(0, 0, -1),
            new Vector3(0, -1, 0),

            new Vector3(0, 0, -1),
            new Vector3(-1, 0, 0),
            new Vector3(0, -1, 0),
        };
        private static Vector3[] Normals = new Vector3[]
        {
            // Top
            new Vector3(0, 1, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 1, 0),

            new Vector3(0, 1, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 1, 0),

            // Sides
            new Vector3(1, -1, 1).Normalized(),
             new Vector3(1, -1, 1).Normalized(),
             new Vector3(1, -1, 1).Normalized(),

            new Vector3(-1, -1, 1).Normalized(),
             new Vector3(-1, -1, 1).Normalized(),
             new Vector3(-1, -1, 1).Normalized(),

            new Vector3(-1, -1, -1).Normalized(),
             new Vector3(-1, -1, -1).Normalized(),
             new Vector3(-1, -1, -1).Normalized(),

            new Vector3(1, -1, -1).Normalized(),
             new Vector3(1, -1, -1).Normalized(),
             new Vector3(1, -1, -1).Normalized(),
        };

        public static void RenderSpawn(float X, float Y, float scale, Vector3 color)
        {
            GL.UseProgram(0);

            GL.PushAttrib(AttribMask.AllAttribBits);
            var position = new Vector3(X, Y, 0);

            GL.Color3(color);
            GL.Begin(PrimitiveType.Triangles);
            for(int i = 0; i <Positions.Length; i++)
            {
                GL.Vertex3(position + Positions[i] * scale);
            }
            GL.End();

            GL.LineWidth(1f);
            GL.Color3(0, 0, 0);
            GL.Begin(PrimitiveType.Lines);
            for (int i = 0; i < Positions.Length; i++)
            {
                GL.Vertex3(position + Positions[i] * scale);
            }
            GL.End();

            GL.PopAttrib();
        }
    }
}
