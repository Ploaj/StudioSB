using System;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;

namespace StudioSB.GUI
{
    public class SBTabPanel : TabControl
    {
        public SBTabPanel()
        {
            ApplicationSettings.SkinControl(this);
        }

        private Dictionary<Control, TabPage> controlToTab = new Dictionary<Control, TabPage>();

        public void ClearTabs()
        {
            TabPages.Clear();
            controlToTab.Clear();
            GC.Collect();
        }

        public void AddTab(string Name, Control c)
        {
            if (controlToTab.ContainsKey(c))
                return;
            TabPage tab = new TabPage();
            tab.Controls.Add(c);
            tab.Text = Name;
            ApplicationSettings.SkinControl(tab);
            TabPages.Add(tab);
            controlToTab.Add(c, tab);
        }

        public void RemoveTab(Control c)
        {
            if (controlToTab.ContainsKey(c))
            {
                TabPages.Remove(controlToTab[c]);
                controlToTab.Remove(c);
            }
        }
    }
}
