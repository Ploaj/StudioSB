using System;
using System.Windows.Forms;

namespace StudioSB.GUI.Menus
{
    public class SBMenuBar : MenuStrip
    {
        public SBMenuBar()
        {
            ApplicationSettings.SkinControl(this);
            Renderer = new SBProfessionalRenderer();
            MinimumSize = new System.Drawing.Size(10, 24);
        }
    }

    public class SBToolStripMenuItem : ToolStripMenuItem
    {
        public SBToolStripMenuItem(string name) : base(name)
        {
            ApplicationSettings.SkinControl(this);
            Width = 150;
        }
    }
}
