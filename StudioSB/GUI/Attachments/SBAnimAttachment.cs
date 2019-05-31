using StudioSB.Scenes.Animation;

namespace StudioSB.GUI.Attachments
{
    public class SBAnimAttachment : IAttachment
    {
        private SBAnimation animation;

        private float PreviousFrame = 0;

        public SBAnimAttachment(SBAnimation animation)
        {
            this.animation = animation;
        }

        public SBAnimation GetAnimation()
        {
            return animation;
        }

        public bool AllowMultiple()
        {
            return false;
        }

        public void AttachToPanel(SBViewportPanel viewportPanel)
        {
            viewportPanel.EnableAnimationBar = true;
            viewportPanel.FrameCount = (int)animation.FrameCount;

        }

        public void RemoveFromPanel(SBViewportPanel viewportPanel)
        {
            viewportPanel.EnableAnimationBar = false;
        }

        public void Render(SBViewport viewport, float frame)
        {
            if (animation == null)
                return;

            if (frame != PreviousFrame)
            {
                animation.UpdateScene(frame, viewport.Scene);
            }
            PreviousFrame = frame;
        }

        public void Step()
        {
        }

        public void Update(SBViewport viewport)
        {
        }
    }
}
