using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace StudioSB.IO.Formats
{
    [Flags]
    public enum SSFEdgeFlags
    {
        None = 0x0,
        Unpaintable = 0x20,
        RightWallOverride = 0x100,
        LeftWallOverride = 0x200,
        CeilingOverride = 0x400,
        FloorOverride = 0x800,
        NoWallJump = 0x1000,
        DropThrough = 0x2000,
        LeftLedge = 0x4000,
        RightLedge = 0x8000,
        IgnoreLinkFromLeft = 0x10000,
        Supersoft = 0x20000,
        IgnoreLinkFromRight = 0x40000,
    }

    [Serializable]
    public class SSF
    {
        public List<SSFPoint> Points = new List<SSFPoint>();

        public List<SSFGroup> Groups = new List<SSFGroup>();

        public static SSF Open(string filePath)
        {
            using (var stream = System.IO.File.OpenRead(filePath))
            {
                var serializer = new XmlSerializer(typeof(SSF));
                return serializer.Deserialize(stream) as SSF;
            }
        }

        public void Save(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SSF));
            TextWriter writer = new StreamWriter(filePath);
            serializer.Serialize(writer, this);
            writer.Close();
        }
    }

    public class SSFPoint
    {
        public float X;
        public float Y;
        public string Tag;
    }

    public class SSFGroup
    {
        public string Name;
        public string JointName;
        public List<SSFVertex> Vertices = new List<SSFVertex>();
        public List<SSFEdge> Edges = new List<SSFEdge>();
    }

    public class SSFEdge
    {
        public int Vertex1;
        public int Vertex2;
        public string Material;
        public SSFEdgeFlags Flags = SSFEdgeFlags.None;
        public string Tags;
    }

    public class SSFVertex
    {
        public float X;
        public float Y;
    }
}
