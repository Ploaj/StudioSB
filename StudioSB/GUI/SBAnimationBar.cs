using System;
using System.Windows.Forms;

namespace StudioSB.GUI
{
    /// <summary>
    /// Animation bar for setting and playing frames
    /// </summary>
    public class SBAnimationBar : Panel
    {
        /// <summary>
        /// read only current frame value
        /// </summary>
        public float Frame
        {
            get => frame.Value;
        }
        private PropertyBinding<float> frame;

        public int FrameCount
        {
            get
            {
                return animationTrack.Maximum;
            }
            set
            {
                animationTrack.Maximum = value;
                frameCountLabel.Text = $"Frame Count: {value}";
            }
        }

        private TrackBar animationTrack;
        private SBButton playButton;
        private SBButton frameAdvance;
        private SBButton frameEnd;
        private SBButton frameRewind;
        private SBButton frameStart;
        private Panel buttonBar;
        private NumericUpDown currentFrame;
        private Label frameLabel;
        private Label frameCountLabel;

        private const string PlayText = "Play";
        private const string PauseText = "Pause";

        public bool IsPlaying { get; internal set; } = false;

        public SBAnimationBar()
        {
            ApplicationSettings.SkinControl(this);

            animationTrack = new TrackBar();
            animationTrack.Dock = DockStyle.Top;
            animationTrack.ValueChanged += FrameChanged;

            buttonBar = new Panel();
            buttonBar.MaximumSize = new System.Drawing.Size(int.MaxValue, 32);
            buttonBar.Dock = DockStyle.Top;
            
            playButton = new SBButton(PlayText);
            playButton.Dock = DockStyle.Fill;
            playButton.Click += PlayPause;
            buttonBar.Controls.Add(playButton);

            frameRewind = new SBButton("<");
            frameRewind.Dock = DockStyle.Left;
            frameRewind.Click += (s, e) => RewindFrame();
            buttonBar.Controls.Add(frameRewind);

            frameStart = new SBButton("<<");
            frameStart.Dock = DockStyle.Left;
            frameStart.Click += (s, e) => animationTrack.Value = 0;
            buttonBar.Controls.Add(frameStart);

            frameAdvance = new SBButton(">");
            frameAdvance.Dock = DockStyle.Right;
            frameAdvance.Click += (s, e) => AdvanceFrame();
            buttonBar.Controls.Add(frameAdvance);

            frameEnd = new SBButton(">>");
            frameEnd.Dock = DockStyle.Right;
            frameEnd.Click += (s, e) => animationTrack.Value = FrameCount;
            buttonBar.Controls.Add(frameEnd);

            //SBHBox hbox = new SBHBox();
            //hbox.MaximumSize = new System.Drawing.Size(1000, 16);
            //hbox.Dock = DockStyle.Top;

            frameLabel = new Label();
            frameLabel.Text = "Frame Count: ";
            frameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //hbox.AddControl(frameLabel);

            currentFrame = new NumericUpDown();
            //hbox.AddControl(currentFrame);
            
            frameCountLabel = new Label();
            frameCountLabel.Text = "";
            frameCountLabel.Dock = DockStyle.Top;
            frameCountLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            //hbox.AddControl(frameCountLabel);

            Controls.Add(buttonBar);
            Controls.Add(animationTrack);
            Controls.Add(frameCountLabel);

            frame = new PropertyBinding<float>();
        }
        
        /// <summary>
        /// Trys to bind an objects frame property, as a float, to this animation bar
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name="PropertyName"></param>
        public void BindFrame(Object obj, string PropertyName)
        {
            frame.UnBind();
            frame.Bind(obj, PropertyName);
        }

        /// <summary>
        /// Updates the frame value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void FrameChanged(object sender, EventArgs args)
        {
            frame.Value = animationTrack.Value;
            //frameLabel.Text = $"Frame: {frame.Value}/{FrameCount}";
        }

        /// <summary>
        /// Event for play/pause being pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
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
        
        /// <summary>
        /// This needs to be constantly running in order to play the animation
        /// </summary>
        public void Process()
        {
            if (IsPlaying)
            {
                AdvanceFrame();
            }
        }

        /// <summary>
        /// Rewinds the frame 1 tick
        /// </summary>
        private void RewindFrame()
        {
            if (animationTrack.Value == 0)
                animationTrack.Value = FrameCount;
            else
                animationTrack.Value--;
        }

        /// <summary>
        /// Advances the frame 1 tick
        /// </summary>
        private void AdvanceFrame()
        {
            if (animationTrack.Value >= FrameCount)
                animationTrack.Value = 0;
            else
                animationTrack.Value++;
        }
    }
}
