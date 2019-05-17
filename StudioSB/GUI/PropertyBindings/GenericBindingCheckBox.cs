using System;
using System.Windows.Forms;

namespace StudioSB.GUI.Editors
{
    /// <summary>
    /// A checkbox with a property binding
    /// </summary>
    public class GenericBindingCheckBox : CheckBox
    {
        private PropertyBinding<bool> _value;

        public GenericBindingCheckBox(string Text) : base()
        {
            ApplicationSettings.SkinControl(this);

            _value = new PropertyBinding<bool>();

            this.Text = Text;

            CheckedChanged += Check2Change;
        }

        public void Bind(object Object, string Property)
        {
            UnBind();
            _value.Bind(Object, Property);

            // so the value isn't updated at this time we unsubscribe from the event temporarily 
            CheckedChanged -= Check2Change;
            Checked = _value.Value;
            CheckedChanged += Check2Change;
        }

        public void UnBind()
        {
            _value.UnBind();
        }

        private void Check2Change(object sender, EventArgs args)
        {
            _value.Value = Checked;
        }
    }
}
