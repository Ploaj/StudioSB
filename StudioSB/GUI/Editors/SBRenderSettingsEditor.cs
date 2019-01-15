using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using StudioSB.GUI.Editors;
using System.Drawing;

namespace StudioSB.GUI
{
    /// <summary>
    /// Does not dispose when closed
    /// </summary>
    public class SBRenderSettingsEditor : Form
    {
        private ToolTip toolTips;
        private SBButton SaveSettings;

        public SBRenderSettingsEditor()
        {
            ApplicationSettings.SkinControl(this);

            Text = "Application Settings";

            TopMost = true;

            Dictionary<SettingsGroupType, SBPopoutPanel> Groups = new Dictionary<SettingsGroupType, SBPopoutPanel>();

            toolTips = new ToolTip();
            // Set up the delays for the ToolTip.
            toolTips.AutoPopDelay = 5000;
            toolTips.InitialDelay = 1000;
            toolTips.ReshowDelay = 500;
            // Force the ToolTip text to be displayed whether or not the form is active.
            toolTips.ShowAlways = true;

            foreach (var e in (SettingsGroupType[])Enum.GetValues(typeof(SettingsGroupType)))
            {
                if (e == SettingsGroupType.Application) continue;
                var panel = new SBPopoutPanel(PopoutSide.Bottom, e.ToString(), e.ToString()) { Dock = DockStyle.Top};
                Groups.Add(e, panel);
                Controls.Add(panel);
            }

            foreach(var prop in typeof(ApplicationSettings).GetProperties().Reverse())
            {
                SettingsGroup attr = (SettingsGroup)prop.GetCustomAttribute(typeof(SettingsGroup));
                if(attr != null && attr.Type != SettingsGroupType.Application)
                {
                    Control control = null;
                    string ExtraLabel = null;
                    if (prop.PropertyType == typeof(Color))
                    {
                        var tb = new GenericColorEditor(attr.Name);
                        tb.Bind(typeof(ApplicationSettings), prop.Name);
                        control = tb;
                    }
                    else
                    if (prop.PropertyType == typeof(int))
                    {
                        var tb = new GenericBindingTextBox<int>();
                        tb.Bind(typeof(ApplicationSettings), prop.Name);
                        control = tb;
                        control.MaximumSize = new Size(64, 32);
                        ExtraLabel = attr.Name + ":";
                    }
                    else
                    if (prop.PropertyType == typeof(bool))
                    {
                        var tb = new GenericBindingCheckBox(attr.Name);
                        tb.Bind(typeof(ApplicationSettings), prop.Name);
                        control = tb;
                    }
                    else
                    if (prop.PropertyType.IsEnum)
                    {
                        // this feel so weird, but it works
                        Type genericClass = typeof(GenericBindingComboBox<>);
                        Type constructedClass = genericClass.MakeGenericType(prop.PropertyType);
                        var tb = (Control)Activator.CreateInstance(constructedClass, prop.Name);
                        tb.GetType().GetMethod("Bind").Invoke(tb, new object[] { typeof(ApplicationSettings), prop.Name });
                        control = tb;
                        control.MaximumSize = new Size(128, 32);
                        ExtraLabel = prop.Name + ":";

                    } else
                        control = new Label() { Text = attr.Name, Dock = DockStyle.Top };

                    toolTips.SetToolTip(control, attr.Description);
                    control.Dock = DockStyle.Top;
                    Groups[attr.Type].Contents.Add(control);
                    if(ExtraLabel != null)
                    {
                        Groups[attr.Type].Contents.Add(new Label() { Text = ExtraLabel, Dock = DockStyle.Top });
                    }
                }
            }
            
            SaveSettings = new SBButton("Save Settings");
            SaveSettings.Dock = DockStyle.Bottom;
            SaveSettings.Click += Editor_SaveSettings;
            Controls.Add(SaveSettings);

            FormClosing += Editor_FormClosing;
        }

        private void Editor_SaveSettings(object sender, EventArgs e)
        {
            ApplicationSettings.SaveSettings();
            Close();
        }

        /// <summary>
        /// Hides this form instead of closing it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Editor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

    }
}
