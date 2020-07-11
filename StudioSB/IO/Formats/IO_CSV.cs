using System.IO;
using System.Text;
using System.Windows.Forms;

namespace StudioSB.IO.Formats
{
    /*class IO_CSV : IExportableModelType
    {
        public string Name => "CSV";
        public string Extension => ".csv";

        bool ExportUV1 = false;
        bool ExportUV2 = false;
        bool ExportUV3 = false;
        bool ExportBoneWeights = false;
        byte UVNum = 1;

        public void ExportIOModel(string FileName, IOModel model)
        {
            DialogResult UV1dialog = MessageBox.Show("Export UV1?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (UV1dialog == DialogResult.Yes)
                ExportUV1 = true;

            DialogResult UV2dialog = MessageBox.Show("Export UV2?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (UV2dialog == DialogResult.Yes)
                ExportUV2 = true;

            DialogResult UV3dialog = MessageBox.Show("Export UV3?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (UV3dialog == DialogResult.Yes)
                ExportUV3 = true;

            DialogResult Bonedialog = MessageBox.Show("Export Bone weights?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (Bonedialog == DialogResult.Yes)
                ExportBoneWeights = true;

            StringBuilder csv = new StringBuilder();

            if (model.HasMeshes)
            {
                foreach (IOMesh mesh in model.Meshes)
                {
                    UVNum = 1;

                    if (ExportUV1 == true)
                        UVNum++;
                    if (ExportUV2 == true)
                        UVNum++;
                    if (ExportUV3 == true)
                        UVNum++;

                    csv.Append($"Obj Name:{mesh.Name}\n");
                    if (ExportBoneWeights == true)
                        if (mesh.HasBoneWeights)
                            csv.Append("Bone_Suport\n");
                    csv.Append($"UV_Num:{UVNum}\n");
                    csv.Append("vert_Array\n");

                    foreach (IOVertex v in mesh.Vertices)
                    {
                        csv.Append($"{v.Position.X},{v.Position.Y},{v.Position.Z}\n");

                        if (mesh.HasNormals)
                            csv.Append($"{v.Normal.X},{v.Normal.Y},{v.Normal.Z}\n");
                        else
                            csv.Append("0.000000,0.000000,1.000000\n");

                        if (mesh.HasColor)
                            //Let's convert these floats to byte range with this dirty trick
                            csv.Append($"{v.Color.X * 255 % 256},{v.Color.Y * 255 % 256},{v.Color.Z * 255 % 256},{v.Color.W * 255 % 256}\n");
                        else
                            csv.Append("127,127,127,255\n");

                        if (mesh.HasUV0)
                            csv.Append($"{v.UV0.X},{v.UV0.Y}\n");
                        else
                            csv.Append("0.000000,0.000000\n");


                        if (ExportUV1 == true)
                        {
                            if (mesh.HasUV1)
                                csv.Append($"{v.UV1.X},{v.UV1.Y}\n");
                            else
                                csv.Append("0.000000,0.000000\n");
                        }

                        if (ExportUV2 == true)
                        {
                            if (mesh.HasUV2)
                                csv.Append($"{v.UV2.X},{v.UV2.Y}\n");
                            else
                                csv.Append("0.000000,0.000000\n");
                        }

                        if (ExportUV3 == true)
                        {
                            if (mesh.HasUV3)
                                csv.Append($"{v.UV3.X},{v.UV3.Y}\n");
                            else
                                csv.Append("0.000000,0.000000\n");
                        }
                    }

                    csv.Append("face_Array\n");

                    for (int i = 0; i < mesh.Indices.Count; i += 3)
                    {
                        csv.Append($"{mesh.Indices[i] + 1},{mesh.Indices[i + 1] + 1},{mesh.Indices[i + 2] + 1}\n");
                    }

                    if (ExportBoneWeights == true)
                    {
                        csv.Append("bone_Array\n");

                        foreach (var v in mesh.Vertices)
                        {
                            if (v.BoneWeights[0] != 0)
                                csv.Append($"{model.Skeleton.Bones[(int)v.BoneIndices.X].Name},{v.BoneWeights[0]},");
                            if (v.BoneWeights[1] != 0)
                                csv.Append($"{model.Skeleton.Bones[(int)v.BoneIndices.Y].Name},{v.BoneWeights[1]},");
                            if (v.BoneWeights[2] != 0)
                                csv.Append($"{model.Skeleton.Bones[(int)v.BoneIndices.Z].Name},{v.BoneWeights[2]},");
                            if (v.BoneWeights[3] != 0)
                                csv.Append($"{model.Skeleton.Bones[(int)v.BoneIndices.W].Name},{v.BoneWeights[3]},");

                            csv.Append("\n");
                        }
                    }
                }
            }
            File.WriteAllText(FileName, csv.ToString());
        }
    }*/
}
