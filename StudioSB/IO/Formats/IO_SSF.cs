using StudioSB.Scenes.LVD;

namespace StudioSB.IO.Formats
{
    public class IO_SSF
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="levelData"></param>
        /// <param name="filePath"></param>
        public static void Export(LevelData levelData, string filePath)
        {
            SSF ssf = new SSF();

            foreach (var collision in levelData.Collisions)
            {
                var g = new SSFGroup();
                ssf.Groups.Add(g);
                g.Name = collision.DynamicName;
                g.JointName = collision.JointName;

                foreach (var v in collision.Vertices)
                {
                    g.Vertices.Add(new SSFVertex() { X = v.X, Y = v.Y });
                }

                for (int i = 0; i < collision.Attributes.Count; i++)
                {
                    g.Edges.Add(new SSFEdge() {
                        Vertex1 = i,
                        Vertex2 = i + 1,
                        Material = collision.Attributes[i].Type.ToString(),
                        Flags = (collision.Attributes[i].Unpaintable ? SSFEdgeFlags.Unpaintable : SSFEdgeFlags.None) |
                                (collision.Attributes[i].RightWallOverride ? SSFEdgeFlags.RightWallOverride : SSFEdgeFlags.None) |
                                (collision.Attributes[i].LeftWallOverride ? SSFEdgeFlags.LeftWallOverride : SSFEdgeFlags.None) |
                                (collision.Attributes[i].CeilingOverride ? SSFEdgeFlags.CeilingOverride : SSFEdgeFlags.None) |
                                (collision.Attributes[i].FloorOverride ? SSFEdgeFlags.FloorOverride : SSFEdgeFlags.None) |
                                (collision.Attributes[i].NoWallJump ? SSFEdgeFlags.NoWallJump : SSFEdgeFlags.None) |
                                (collision.Attributes[i].DropThrough ? SSFEdgeFlags.DropThrough : SSFEdgeFlags.None) |
                                (collision.Attributes[i].LeftLedge ? SSFEdgeFlags.LeftLedge : SSFEdgeFlags.None) |
                                (collision.Attributes[i].RightLedge ? SSFEdgeFlags.RightLedge : SSFEdgeFlags.None) |
                                (collision.Attributes[i].IgnoreLinkFromLeft ? SSFEdgeFlags.IgnoreLinkFromLeft : SSFEdgeFlags.None) |
                                (collision.Attributes[i].Supersoft ? SSFEdgeFlags.Supersoft : SSFEdgeFlags.None) |
                                (collision.Attributes[i].IgnoreLinkFromRight ? SSFEdgeFlags.IgnoreLinkFromRight : SSFEdgeFlags.None),
                    });
                }
            }

            int index = 0;

            foreach (var v in levelData.StartPositions)
            {
                ssf.Points.Add(new SSFPoint()
                {
                    Tag = "StartPosition" + index++,
                    X = v.Position.X,
                    Y = v.Position.Y
                });
            }

            index = 0;

            foreach (var v in levelData.RestartPositions)
            {
                ssf.Points.Add(new SSFPoint()
                {
                    Tag = "RestartPosition" + index++,
                    X = v.Position.X,
                    Y = v.Position.Y
                });
            }

            index = 0;

            foreach (var v in levelData.CameraRegions)
            {
                ssf.Points.Add(new SSFPoint()
                {
                    Tag = "CameraRegionStart" + index++,
                    X = v.Rectangle.Left,
                    Y = v.Rectangle.Top
                });
                ssf.Points.Add(new SSFPoint()
                {
                    Tag = "CameraRegionEnd" + index++,
                    X = v.Rectangle.Right,
                    Y = v.Rectangle.Bottom
                });
            }

            index = 0;

            foreach (var v in levelData.DeathRegions)
            {
                ssf.Points.Add(new SSFPoint()
                {
                    Tag = "DeathRegionStart" + index++,
                    X = v.Rectangle.Left,
                    Y = v.Rectangle.Top
                });
                ssf.Points.Add(new SSFPoint()
                {
                    Tag = "DeathRegionEnd" + index++,
                    X = v.Rectangle.Right,
                    Y = v.Rectangle.Bottom
                });
            }

            ssf.Save(filePath);
        }
    }
}
