using StudioSB.Scenes.Animation;
using System.Windows.Forms;

namespace StudioSB.GUI.Attachments
{
    public class SBAnimAttachment : IAttachment
    {
        private SBAnimationBar animationBar;
        private SBAnimation animation;

        private float PreviousFrame = 0;

        public SBAnimAttachment(SBAnimation animation)
        {
            this.animation = animation;

            animationBar = new SBAnimationBar();
            animationBar.Dock = DockStyle.Bottom;

            animationBar.FrameCount = (int)animation.FrameCount;
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
            if(!viewportPanel.Controls.Contains(animationBar))
                viewportPanel.Controls.Add(animationBar);
        }

        public void RemoveFromPanel(SBViewportPanel viewportPanel)
        {
            if (viewportPanel.Controls.Contains(animationBar))
                viewportPanel.Controls.Remove(animationBar);
        }

        public void Render(SBViewport viewport)
        {
            if (viewport.Scene == null || animation == null)
                return;

            animationBar.Process();
            if (animationBar.Frame != PreviousFrame)
            {
                animation.UpdateScene(animationBar.Frame, viewport.Scene);
            }
            PreviousFrame = animationBar.Frame;
        }

        public void Step()
        {
        }

        public void Update(SBViewport viewport)
        {
        }
    }
}
