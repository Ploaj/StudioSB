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
