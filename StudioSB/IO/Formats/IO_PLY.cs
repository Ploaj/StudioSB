using System.Text;
using StudioSB.IO.Models;
using System.IO;

namespace StudioSB.IO.Formats
{
    public class IO_PLY : IExportableModelType
    {
        public string Name => "PLY";
        public string Extension => ".ply";

        public void ExportIOModel(string FileName, IOModel model)
        {
            int num = 0;
            string filepath = Path.GetDirectoryName(FileName);
            string filename = Path.GetFileNameWithoutExtension(FileName);

            if (model.HasMeshes)
            {
                //.ply only support a sigle mesh, So export each mesh as a seperate file
                foreach (IOMesh mesh in model.Meshes)
                {
                    StringBuilder o = new StringBuilder();

                    //Using \n instead of AppendLine because it adds a white space, no text editor can see.
                    //No idea why. Opening it in a hex editor shows 0D
                    //.ply doesn't support white spaces unless it's a comment

                    o.Append("ply\n");
                    o.Append("format ascii 1.0\n");
                    o.Append("comment Created by CrossMod: https://github.com/Ploaj/CrossMod/ \n");
                    o.Append($"element vertex {mesh.Vertices.Count}\n");

                    o.Append("property float x\n" +
                             "property float y\n" +
                             "property float z\n");

                    if (mesh.HasNormals)
                    {
                        o.Append("property float nx\n" +
                                 "property float ny\n" +
                                 "property float nz\n");
                    }
                    if (mesh.HasUV0)
                    {
                        o.Append("property float s\n" +
                                     "property float t\n");
                    }
                    if (mesh.HasColor)
                    {
                        o.Append("property uchar red\n" +
                                 "property uchar green\n" +
                                 "property uchar blue\n" +
                                 "property uchar alpha\n");
                    }

                    //Divide vertex count by 3 to get triangle count
                    o.Append($"element face {mesh.Indices.Count / 3}\n");
                    o.Append("property list uchar uint vertex_indices\n");
                    o.Append("end_header\n");

                    foreach (IOVertex v in mesh.Vertices)
                    {
                        o.Append($"{v.Position.X} {v.Position.Y} {v.Position.Z}");

                        if (mesh.HasNormals)
                            o.Append($" {v.Normal.X} {v.Normal.Y} {v.Normal.Z}");

                        if (mesh.HasUV0)
                            o.Append($" {v.UV0.X} {v.UV0.Y}");

                        if (mesh.HasColor)
                            o.Append($" {v.Color.X} {v.Color.Y} {v.Color.Z} {v.Color.W}");

                        o.Append("\n");
                    }

                    for (int i = 0; i < mesh.Indices.Count; i += 3)
                    {
                        o.Append($"3 {mesh.Indices[i]} {mesh.Indices[i + 1]} {mesh.Indices[i + 2]}\n");
                    }

                    File.WriteAllText($"{filepath}\\{filename}_{num++}.ply", o.ToString());
                }
            }
        }
    }
}
