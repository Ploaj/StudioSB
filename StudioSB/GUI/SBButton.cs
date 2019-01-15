using System;
using System.Windows.Forms;

namespace StudioSB.GUI
{
    public class SBButton : Button
    {
        public SBButton(string v)
        {
            ApplicationSettings.SkinControl(this);
            Text = v;
        }
    }
}
