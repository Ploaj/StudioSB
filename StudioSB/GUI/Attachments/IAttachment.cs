namespace StudioSB.GUI.Attachments
{
    public interface IAttachment
    {
        bool AllowMultiple();

        void AttachToPanel(SBViewportPanel viewportPanel);

        void RemoveFromPanel(SBViewportPanel viewportPanel);

        void Update(SBViewport viewport);

        void Step();

        void Render(SBViewport viewport, float frame = 0);
    }
}
