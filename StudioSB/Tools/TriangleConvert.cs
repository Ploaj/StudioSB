using IONET.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioSB.Tools
{
    public class TriangleConvert
    {
        /// <summary>
        /// Converts a list of quads into triangles
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="outVertices"></param>
        public static void QuadToList<T>(List<T> vertices, out List<T> outVertices)
        {
            outVertices = new List<T>();

            for (int index = 0; index < vertices.Count; index += 4)
            {
                outVertices.Add(vertices[index]);
                outVertices.Add(vertices[index + 1]);
                outVertices.Add(vertices[index + 2]);
                outVertices.Add(vertices[index + 1]);
                outVertices.Add(vertices[index + 3]);
                outVertices.Add(vertices[index + 2]);
            }
        }

        /// <summary>
        /// Converts a list of triangle strips into triangles
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="outVertices"></param>
        public static void StripToList(List<IOVertex> vertices, out List<IOVertex> outVertices)
        {
            outVertices = new List<IOVertex>();

            for (int index = 2; index < vertices.Count; index++)
            {
                bool isEven = (index % 2 != 1);

                var vert1 = vertices[index - 2];
                var vert2 = isEven ? vertices[index] : vertices[index - 1];
                var vert3 = isEven ? vertices[index - 1] : vertices[index];

                if (!vert1.Position.Equals(vert2.Position) && !vert2.Position.Equals(vert3.Position) && !vert3.Position.Equals(vert1.Position))
                {
                    outVertices.Add(vert3);
                    outVertices.Add(vert2);
                    outVertices.Add(vert1);
                }
                else
                {
                    //Console.WriteLine("ignoring degenerate triangle");
                }
            }
        }

        /// <summary>
        /// reverses faces for triangle lists only
        /// </summary>
        /// <param name="triangles"></param>
        /// <param name="reversed"></param>
        public static void ReverseFaces(List<uint> triangles, out List<uint> reversed)
        {
            reversed = new List<uint>(triangles.Count);

            for (int i = 0; i < triangles.Count; i += 3)
            {
                reversed.Add(triangles[i + 2]);
                reversed.Add(triangles[i + 1]);
                reversed.Add(triangles[i]);
            }
        }
    }
}
