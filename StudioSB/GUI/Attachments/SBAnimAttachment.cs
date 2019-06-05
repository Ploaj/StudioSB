using StudioSB.Rendering.Bounding;
using StudioSB.Scenes.Animation;

namespace StudioSB.GUI.Attachments
{
    public class SBAnimAttachment : IAttachment
    {
        private SBAnimation animation;

        private float PreviousFrame = 0;

        public SBAnimAttachment()
        {

        }

        public SBAnimAttachment(SBAnimation animation)
        {
            this.animation = animation;
        }

        public SBAnimation GetAnimation()
        {
            return animation;
        }

        public bool OverlayScene()
        {
            return true;
        }


        public bool AllowMultiple()
        {
            return false;
        }

        public void OnAttach(SBViewportPanel viewportPanel)
        {
            viewportPanel.EnableAnimationBar = true;
            viewportPanel.FrameCount = (int)animation.FrameCount;

        }

        public void OnRemove(SBViewportPanel viewportPanel)
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

        public void Step(SBViewport viewport)
        {
        }

        public void Update(SBViewport viewport)
        {
        }

        public void Pick(Ray ray)
        {

        }

        public string[] Extension()
        {
            return null;
        }

        public void Open(string FilePath)
        {
        }

        public void Save(string FilePath)
        {
        }
    }
}
