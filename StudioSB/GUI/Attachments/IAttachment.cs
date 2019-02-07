namespace StudioSB.GUI.Attachments
{
    public interface IAttachment
    {
        void AttachToPanel(SBViewportPanel viewportPanel);

        void Update(SBViewport viewport);

        void Step();

        void Render(SBViewport viewport);
    }
}
