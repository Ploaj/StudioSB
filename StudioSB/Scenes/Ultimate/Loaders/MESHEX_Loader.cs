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
                    TrueName = Name.IndexOf("_VIS") != -1 ? Name.Substring(0, Name.IndexOf("_VIS")) : Name
                });
            }

            Entries.Add(new EntryEx() { MeshExIndex = data });
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
