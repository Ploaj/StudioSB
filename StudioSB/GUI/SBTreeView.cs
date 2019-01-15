using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StudioSB.GUI
{
    public class SBTreeView : TreeView
    {
        public SBTreeView()
        {
            ApplicationSettings.SkinControl(this);
        }
    }
}
