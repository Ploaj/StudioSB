using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;

namespace StudioSB.GUI
{
    internal class FilteredFileNameEditor : UITypeEditor
    {
        private OpenFileDialog ofd = new OpenFileDialog();
        public override UITypeEditorEditStyle GetEditStyle(
         ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(
         ITypeDescriptorContext context,
         IServiceProvider provider,
         object value)
        {
            ofd.FileName = value.ToString();
            //ofd.Filter = "Text File|*.txt|All Files|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                return ofd.FileName;
            }
            return base.EditValue(context, provider, value);
        }
    }

    public class SBCustomDialog : Form
    {
        private object Value;

        private PropertyGrid propGrid;

        private SBButton Done;

        public SBCustomDialog(object Value)
        {
            this.Value = Value;

            Text = "Settings";

            Done = new SBButton("Okay");
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
