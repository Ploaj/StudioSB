namespace StudioSB.Scenes
{
    /// <summary>
    /// Interface for mesh object
    /// </summary>
    public class ISBMesh
    {
        public string Name { get; set; } = "";

        public string ParentBone { get; set; }
        
        public ISBMaterial Material { get; set; }

        #region GUI interfacing stuff
        public bool Visible { get; set; }

        public bool Selected { get; set; }

        public virtual int PolyCount { get; }

        public virtual int VertexCount { get; }

        #endregion
    }
}
