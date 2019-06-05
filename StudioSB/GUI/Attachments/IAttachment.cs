using StudioSB.Rendering.Bounding;

namespace StudioSB.GUI.Attachments
{
    public interface IAttachment
    {
        string[] Extension();

        bool AllowMultiple();

        bool OverlayScene();

        void Open(string FilePath);

        void Save(string FilePath);

        void OnAttach(SBViewportPanel viewportPanel);

        void OnRemove(SBViewportPanel viewportPanel);

        void Pick(Ray ray);

        void Update(SBViewport viewport);

        void Step(SBViewport viewport);

        void Render(SBViewport viewport, float frame = 0);
    }
}
