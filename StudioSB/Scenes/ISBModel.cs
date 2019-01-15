using System.Collections.Generic;
using OpenTK;

namespace StudioSB.Scenes
{
    /// <summary>
    /// The interface for scene models
    /// </summary>
    public interface ISBModel<T>
    {
        string Name { get; set; }
        
        List<T> Meshes { get; set; }

        Vector4 BoundingSphere { get; set; }
    }
}
