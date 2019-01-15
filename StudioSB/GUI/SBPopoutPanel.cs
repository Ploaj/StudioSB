using System;
using System.Windows.Forms;

namespace StudioSB.GUI
{
    public enum PopoutSide
    {
        Right,
        Left,
        Top,
        Bottom
    }

    public class SBPopoutPanel : Panel
    {
        public string OpenText { get; set; }
        public string CloseText { get; set; }

        private SBButton _popOutButton;
        private bool Expanded = false;

        public ControlCollection Contents
        {
            get
            {
                return _contentPanel.Controls;
            }
        }

        private Panel _contentPanel;
        public PopoutSide PopoutSide { get; internal set; }

        public SBPopoutPanel(PopoutSide PopoutSide, string OpenText = "Open", string CloseText = "Close")
        {
            ApplicationSettings.SkinControl(this);

            this.OpenText = OpenText;
            this.CloseText = CloseText;
            this.PopoutSide = PopoutSide;

            _contentPanel = new Panel();

            if (PopoutSide == PopoutSide.Top)
            {
                _contentPanel.Dock = DockStyle.Top;
                _popOutButton = new SBButton(OpenText);
                _popOutButton.Dock = DockStyle.Bottom;
                _popOutButton.MaximumSize = new System.Drawing.Size(int.MaxValue, 24);
            }
            else
            if (PopoutSide == PopoutSide.Bottom)
            {
                _contentPanel.Dock = DockStyle.Bottom;
                _popOutButton = new SBButton(OpenText);
                _popOutButton.Dock = DockStyle.Top;
                _popOutButton.MaximumSize = new System.Drawing.Size(int.MaxValue, 24);
            }
            else
            if (PopoutSide == PopoutSide.Right)
            {
                _contentPanel.Dock = DockStyle.Right;
                _popOutButton = new SBButton(OpenText);
                _popOutButton.Dock = DockStyle.Left;
                _popOutButton.MaximumSize = new System.Drawing.Size(24, int.MaxValue);
            }
            else
            {
                _contentPanel.Dock = DockStyle.Left;
                _popOutButton = new SBButton(OpenText);
                _popOutButton.Dock = DockStyle.Right;
                _popOutButton.MaximumSize = new System.Drawing.Size(24, int.MaxValue);
            }
            
            _popOutButton.Click += Expand;
            _popOutButton.BackColor = ApplicationSettings.PoppedInColor;

            //_contentPanel.BackColor = ApplicationSettings.MiddleColor;
            _contentPanel.MinimumSize = new System.Drawing.Size(280, 32);
            _contentPanel.MaximumSize = new System.Drawing.Size(int.MaxValue, int.MaxValue);
            //_contentPanel.AutoSize = true;
            _contentPanel.AutoScroll = true;

            Controls.Add(_popOutButton);
            Controls.Add(_contentPanel);

            if (PopoutSide == PopoutSide.Left || PopoutSide == PopoutSide.Right)
                Width = _popOutButton.Width;
            else
                Height = _popOutButton.Height;
        }

        private new void Resize()
        {
            _contentPanel.Size = Size;
        }

        private void Expand(object sender, EventArgs args)
        {
            if (Expanded)
            {
                Expanded = false;
                _popOutButton.Text = OpenText;
                _popOutButton.BackColor = ApplicationSettings.PoppedInColor;
                if (PopoutSide == PopoutSide.Left || PopoutSide == PopoutSide.Right)
                {
                    Width = _popOutButton.Width;
                }
                else
                {
                    Height = _popOutButton.Height;
                }
            }
            else
            {
                Expanded = true;
                _popOutButton.Text = CloseText;
                _popOutButton.BackColor = ApplicationSettings.PoppedOutColor;

                if (PopoutSide == PopoutSide.Left || PopoutSide == PopoutSide.Right)
                {
                    _popOutButton.Text = PopoutSide == PopoutSide.Right ? ">" : "<";
                    Width = _popOutButton.Width + _contentPanel.Width;
                }
                else
                {
                    Height = _popOutButton.Height + _contentPanel.Height;
                }
                }
        }
    }
}
