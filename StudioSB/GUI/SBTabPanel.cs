using System;
using System.Windows.Forms;

namespace StudioSB.GUI
{
    public class SBTabPanel : TabControl
    {
        public SBTabPanel()
        {
            ApplicationSettings.SkinControl(this);
        }

        public void ClearTabs()
        {
            TabPages.Clear();
            GC.Collect();
        }

        public void AddTab(string Name, Control c)
        {
            TabPage tab = new TabPage();
            tab.Controls.Add(c);
            tab.Text = Name;
            ApplicationSettings.SkinControl(tab);
            TabPages.Add(tab);
        }
    }
}
