using System;
using System.Windows.Forms;

namespace StudioSB.GUI
{
    public class SBCustomDialog : Form
    {
        private object Value;

        private PropertyGrid propGrid;

        private SBButton Done;

        public SBCustomDialog(object Value)
        {
            this.Value = Value;

            Text = "Settings";

            Done = new SBButton("Save");
            Done.Dock = DockStyle.Bottom;
            Done.Click += delegate
            {
                DialogResult = DialogResult.OK;
                Close();
            };
            Controls.Add(Done);

            propGrid = new PropertyGrid();
            ApplicationSettings.SkinControl(propGrid);
            propGrid.SelectedObject = Value;
            propGrid.Dock = DockStyle.Fill;
            Controls.Add(propGrid);

        }
        

    }
}
