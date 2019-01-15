using System.Drawing;
using System.Windows.Forms;

namespace StudioSB.GUI.Menus
{
    public class SBProfessionalRenderer : ToolStripProfessionalRenderer
    {
        public SBProfessionalRenderer() : base(new SBProfessionalColors()) { }
    }

    public class SBProfessionalColors : ProfessionalColorTable
    {
        public override Color MenuItemSelected
        {
            get { return ApplicationSettings.SeletectedToolColor; }
        }
        public override Color MenuItemSelectedGradientBegin
        {
            get { return ApplicationSettings.BackgroundColor; }
        }
        public override Color MenuItemSelectedGradientEnd
        {
            get { return ApplicationSettings.BackgroundColor; }
        }

        public override Color MenuItemPressedGradientBegin
        {
            get { return ApplicationSettings.SeletectedToolColor; }
        }

        public override Color MenuItemPressedGradientEnd
        {
            get { return ApplicationSettings.SeletectedToolColor; }
        }
    }
}
