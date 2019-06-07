using StudioSB.Rendering.Bounding;

namespace StudioSB.GUI.Attachments
{
    public interface IAttachment
    {
        /// <summary>
        /// supported open extension for this attachment
        /// null if this attachment doesn't support a type
        /// </summary>
        /// <returns></returns>
        string[] Extension();

        /// <summary>
        /// True if multiple of these attachments are allowed to exist
        /// </summary>
        /// <returns></returns>
        bool AllowMultiple();

        /// <summary>
        /// True if this attachment can be opened over a scene
        /// </summary>
        /// <returns></returns>
        bool OverlayScene();

        /// <summary>
        /// Opening code for the file type supported by this attachment
        /// </summary>
        /// <param name="FilePath"></param>
        void Open(string FilePath);

        /// <summary>
        /// Exporting code for the file type supported by this attachment
        /// </summary>
        /// <param name="FilePath"></param>
        void Save(string FilePath);

        /// <summary>
        /// Executes when the attachment is attached to the scene
        /// Initialization for attachment is done here
        /// </summary>
        /// <param name="viewportPanel"></param>
        void OnAttach(SBViewportPanel viewportPanel);

        /// <summary>
        /// Executes when the attachment is removed from the scene
        /// Cleanup for attachment is done here
        /// </summary>
        /// <param name="viewportPanel"></param>
        void OnRemove(SBViewportPanel viewportPanel);

        /// <summary>
        /// Function for screne picking
        /// </summary>
        /// <param name="ray">The pick ray in world space</param>
        void Pick(Ray ray);

        /// <summary>
        /// Updates information in this attachment to match current scene state
        /// </summary>
        /// <param name="viewport"></param>
        void Update(SBViewport viewport);

        /// <summary>
        /// Runs every idle frame, put any looping logic code here
        /// </summary>
        /// <param name="viewport"></param>
        void Step(SBViewport viewport);

        /// <summary>
        /// Renders to the scene
        /// </summary>
        /// <param name="viewport"></param>
        /// <param name="frame"></param>
        void Render(SBViewport viewport, float frame = 0);
    }
}
