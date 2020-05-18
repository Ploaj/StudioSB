using StudioSB.Scenes.LVD;

namespace StudioSB.IO.Formats
{
    public class IO_SSF
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lvd"></param>
        /// <param name="filePath"></param>
        public static void Export(LevelData lvd, string filePath)
        {
            SSF ssf = new SSF();

            foreach(var v in lvd.Collisions)
            {
                var g = new SSFGroup();
                ssf.Groups.Add(g);
                g.Name = v.EntryLabel;
                g.Bone = v.BoneName;
                foreach(var x in v.Vertices)
                    g.Vertices.Add(new SSFVertex() { X = x.X, Y = x.Y });
                for(int i = 0; i < v.Materials.Count; i++)
                {
                    g.Lines.Add(new SSFLine() {
                        Vertex1 = i,
                        Vertex2 = i + 1,
                        Material = v.Materials[i].Physics.ToString(),
                        Flags = (v.Materials[i].LeftLedge ? SSFLineFlag.LeftLedge : 0) |
                        (v.Materials[i].RightLedge ? SSFLineFlag.RightLedge : 0) |
                        (v.PassThrough ? SSFLineFlag.DropThrough : 0)
                    });
                }
            }

            foreach (var v in lvd.BlastZoneBounds)
            {
                ssf.Points.Add(new SSFPoint()
                {
                    Tag = "BlastStart",
                    X = v.Left,
                    Y = v.Top
                });
                ssf.Points.Add(new SSFPoint()
                {
                    Tag = "BlastEnd",
                    X = v.Right,
                    Y = v.Bottom
                });
            }

            foreach (var v in lvd.CameraBounds)
            {
                ssf.Points.Add(new SSFPoint()
                {
                    Tag = "CameraStart",
                    X = v.Left,
                    Y = v.Top
                });
                ssf.Points.Add(new SSFPoint()
                {
                    Tag = "CameraEnd",
                    X = v.Right,
                    Y = v.Bottom
                });
            }

            int sIndex = 0;
            foreach (var v in lvd.Spawns)
            {
                ssf.Points.Add(new SSFPoint()
                {
                    Tag = "Spawn" + sIndex++,
                    X = v.X,
                    Y = v.Y
                });
            }
            sIndex = 0;
            foreach (var v in lvd.Respawns)
            {
                ssf.Points.Add(new SSFPoint()
                {
                    Tag = "Respawn" + sIndex++,
                    X = v.X,
                    Y = v.Y
                });
            }

            ssf.Save(filePath);
        }
    }


}
