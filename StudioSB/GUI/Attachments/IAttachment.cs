using StudioSB.Rendering.Bounding;

namespace StudioSB.GUI.Attachments
{
    public interface IAttachment
    {
        bool AllowMultiple();

        void AttachToPanel(SBViewportPanel viewportPanel);

        void RemoveFromPanel(SBViewportPanel viewportPanel);

        void Update(SBViewport viewport);

        void Step();

        void Pick(Ray ray);

        void Render(SBViewport viewport, float frame = 0);
    }
}
