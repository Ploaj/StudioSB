using System.Windows.Forms;

namespace StudioSB.GUI
{
    public class SBListView : ListView
    {
        public SBListView() : base()
        {
            ApplicationSettings.SkinControl(this);
        }
    }
}