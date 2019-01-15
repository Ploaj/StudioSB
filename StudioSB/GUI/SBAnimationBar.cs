using System;
using System.Windows.Forms;

namespace StudioSB.GUI
{
    public class SBAnimationBar : Panel
    {
        public PropertyBinding<float> Frame;

        public int FrameCount
        {
            get
            {
                return animationTrack.Maximum;
            }
            set
            {
                animationTrack.Maximum = value;
            }
        }

        private TrackBar animationTrack;
        private SBButton playButton;

        private const string PlayText = "Play";
        private const string PauseText = "Pause";

        public bool IsPlaying { get; internal set; } = false;

        public SBAnimationBar()
        {
            ApplicationSettings.SkinControl(this);

            animationTrack = new TrackBar();
            animationTrack.Dock = DockStyle.Top;
            animationTrack.ValueChanged += FrameChanged;

            playButton = new SBButton(PlayText);
            playButton.Dock = DockStyle.Top;
            playButton.Click += PlayPause;

            Controls.Add(playButton);
            Controls.Add(animationTrack);

            Frame = new PropertyBinding<float>();
        }


        private void FrameChanged(object sender, EventArgs args)
        {
            Frame.Value = animationTrack.Value;
        }

        private void PlayPause(object sender, EventArgs args)
        {
            if (IsPlaying)
            {
                IsPlaying = false;
                playButton.Text = PlayText;
            }
            else
            {
                IsPlaying = true;
                playButton.Text = PauseText;
            }
        }
        
        public void Process()
        {
            if (IsPlaying)
            {
                if (animationTrack.Value >= FrameCount)
                    animationTrack.Value = 0;
                else
                    animationTrack.Value++;
            }
        }

    }
}
