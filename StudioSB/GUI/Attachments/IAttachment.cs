using StudioSB.Rendering.Bounding;

namespace StudioSB.GUI.Attachments
{
    public interface IAttachment
    {
        string Extension();

        bool AllowMultiple();

        void Open(string FilePath);

        void Save(string FilePath);

        void AttachToPanel(SBViewportPanel viewportPanel);

        void RemoveFromPanel(SBViewportPanel viewportPanel);

        void Pick(Ray ray);

        void Update(SBViewport viewport);

        void Step(SBViewport viewport);

        void Render(SBViewport viewport, float frame = 0);
    }
}
