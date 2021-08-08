using OpenTK;
using StudioSB.Rendering.Bounding;
using StudioSB.Tools;
using System;
using System.Collections.Generic;
using System.IO;

namespace StudioSB.Scenes.Ultimate.Loaders
{
    public class MESHEX_Loader
    {
        public BoundingSphere AllBoundingSphere;
        public string AllName = "All";
        public List<MeshEX> MeshData = new List<MeshEX>();
        public List<EntryEx> Entries = new List<EntryEx>();

        public class MeshEX
        {
            public BoundingSphere BoundingSphere;
            public string Name;
            public string TrueName;
        }

        public class EntryEx
        {
            public int MeshExIndex;
            public short Flag = 3;
            public Vector3 Unknown = Vector3.UnitY;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bounding"></param>
        /// <param name="Name"></param>
        public void AddMeshData(BoundingSphere bounding, string Name)
        {
            var data = MeshData.FindIndex(e=>e.Name.Equals(Name));

            if(data == -1)
            {
                data = MeshData.Count;
                MeshData.Add(new MeshEX()
                {
                    BoundingSphere = bounding,
                    Name = Name,
                    TrueName = GetTrueName(Name)
                });
            }
    
            Entries.Add(new EntryEx() { MeshExIndex = data });
        }

        /// <summary>
        /// For each mesh in meshes, we will add an entry to 'Entries'. However, all meshes of the same name will get consolidated into
        /// one single 'MeshData' entry that contains a new bounding sphere that encapsulates all the same-named meshes
        /// </summary>
        public void AddAllMeshData(List<SBUltimateMesh> Meshes)
        {
            Dictionary<string, List<SBUltimateMesh>> mesh_name_dict = new Dictionary<string, List<SBUltimateMesh>>();
            foreach (var mesh in Meshes)
            {
                List<SBUltimateMesh> mesh_list = new List<SBUltimateMesh>();
                if (mesh_name_dict.TryGetValue(mesh.Name, out mesh_list))
                {
                    mesh_list.Add(mesh);
                }
                else
                {
                    List<SBUltimateMesh> new_mesh_list = new List<SBUltimateMesh>();
                    new_mesh_list.Add(mesh);
                    mesh_name_dict.Add(mesh.Name, new_mesh_list);
                }
            }

            foreach (var unique_mesh_name in mesh_name_dict.Keys)
            {
                List<SBUltimateMesh> mesh_list = mesh_name_dict[unique_mesh_name];
                List<Vector3> grouped_vertices = new List<Vector3>();
                foreach (var mesh in mesh_list)
                {
                    foreach (var vertex in mesh.Vertices)
                    {
                        grouped_vertices.Add(vertex.Position0);
                    }
                }
                BoundingSphere group_sphere = new BoundingSphere(grouped_vertices);
                MeshData.Add(new MeshEX()
                {
                    BoundingSphere = group_sphere,
                    Name = unique_mesh_name,
                    TrueName = GetTrueName(unique_mesh_name)
                });   
            }

            foreach (var mesh in Meshes)
            {
                var data_index = MeshData.FindIndex(e => e.Name.Equals(mesh.Name));
                Entries.Add(new EntryEx() { MeshExIndex = data_index });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string GetTrueName(string name)
        {
            if (name.Contains("_VIS_"))
                name = name.Substring(0, name.IndexOf("_VIS_"));
            if (name.Contains("_O_"))
                name = name.Substring(0, name.IndexOf("_O_"));
            if (name.EndsWith("Shape"))
                name = name.Substring(0, name.IndexOf("Shape"));

            return name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        public void Save(String filePath)
        {
            using (BinaryWriterExt w = new BinaryWriterExt(new FileStream(filePath, FileMode.Create)))
            {
                w.BigEndian = false;
                w.Write(new byte[0x40]);

                var nameStart = w.Position;
                w.Write(AllBoundingSphere.X);
                w.Write(AllBoundingSphere.Y);
                w.Write(AllBoundingSphere.Z);
                w.Write(AllBoundingSphere.Radius);
                w.Write((long)0x60);
                w.Write((long)0x0);
                w.Write(AllName.ToCharArray());
                w.Write((byte)0);
                if (w.Position % 0x4 != 0)
                    w.Write(new byte[0x4 - w.Position % 0x4]);

                if (w.Position % 0x10 != 0)
                    w.Write(new byte[0x10 - w.Position % 0x10]);


                var meshStart = w.Position;
                w.Write(new byte[MeshData.Count * 0x20]);
                var stringPos = w.Position;
                for(int i = 0; i < MeshData.Count; i++)
                {
                    w.Position = stringPos;

                    long nameLoc = w.Position;
                    w.Write(MeshData[i].Name.ToCharArray());
                    w.Write((byte)0);
                    if (w.Position % 0x4 != 0)
                        w.Write(new byte[0x4 - w.Position % 0x4]);

                    long actualnameLoc = w.Position;
                    w.Write(MeshData[i].TrueName.ToCharArray());
                    w.Write((byte)0);
                    if (w.Position % 0x4 != 0)
                        w.Write(new byte[0x4 - w.Position % 0x4]);

                    stringPos = w.Position;

                    w.Position = (uint)(meshStart + i * 0x20);
                    w.Write(MeshData[i].BoundingSphere.X);
                    w.Write(MeshData[i].BoundingSphere.Y);
                    w.Write(MeshData[i].BoundingSphere.Z);
                    w.Write(MeshData[i].BoundingSphere.Radius);
                    w.Write(nameLoc);
                    w.Write(actualnameLoc);
                }
                w.Position = stringPos;

                if (w.Position % 0x10 != 0)
                    w.Write(new byte[0x10 - w.Position % 0x10]);
                
                var entryStart = w.Position;

                foreach(var e in Entries)
                {
                    w.Write(e.MeshExIndex);
                    w.Write(e.Unknown.X);
                    w.Write(e.Unknown.Y);
                    w.Write(e.Unknown.Z);
                }

                if (w.Position % 0x10 != 0)
                    w.Write(new byte[0x10 - w.Position % 0x10]);


                var flagStart = w.Position;
                
                foreach (var e in Entries)
                {
                    w.Write(e.Flag);
                }

                if (w.Position % 0x10 != 0)
                    w.Write(new byte[0x10 - w.Position % 0x10]);

                w.Position = 0;
                w.Write((long)w.BaseStream.Length);
                w.Write(Entries.Count);
                w.Write(MeshData.Count);
                w.Write((long)nameStart);
                w.Write((long)meshStart);
                w.Write((long)entryStart);
                w.Write((long)flagStart);
            }
        }
    }
}
