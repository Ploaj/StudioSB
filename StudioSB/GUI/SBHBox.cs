using System;
using System.Windows.Forms;

namespace StudioSB.GUI
{
    /// <summary>
    /// Horizontal panel layout
    /// </summary>
    public class SBHBox : TableLayoutPanel
    {
        private int index = 0;

        public SBHBox()
        {
            ApplicationSettings.SkinControl(this);

            AutoSize = true;
        }

        /// <summary>
        /// Adds a new control to HBox
        /// </summary>
        /// <param name="c"></param>
        public void AddControl(Control c)
        {
            Controls.Add(c, index++, 0);
        }

    }
}
