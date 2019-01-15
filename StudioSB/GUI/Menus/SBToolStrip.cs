using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;

namespace StudioSB.GUI.Menus
{
    public class SBToolStrip : ToolStrip
    {
        public SBToolStrip()
        {
            ApplicationSettings.SkinControl(this);
        }
    }
    
    public class SBToolStripButton : ToolStripButton
    {
        public SBToolStripButton(string Name) : base(Name)
        {
            ApplicationSettings.SkinControl(this);
        }
    }

    public class SBToolStripFloatBox : ToolStripTextBox
    {
        public float Value;

        public PropertyInfo Property;
        public object Bind;

        public SBToolStripFloatBox() : base()
        {
            //ApplicationSettings.SetupControl(this);

            TextChanged += ValueChanged;
        }

        public void Unbind()
        {
            BindValue(null, null);
        }

        public void BindValue(object Object, string PropertyName)
        {
            if (Object == null)
            {
                Bind = null;
                Property = null;
                return;
            }
            foreach(var prop in Object.GetType().GetProperties())
            {
                if (prop.Name.Equals(PropertyName))
                {
                    Bind = Object;
                    Property = prop;
                    Text = Property.GetValue(Object).ToString();
                    break;
                }
            }
        }

        private void ValueChanged(object sender, EventArgs args)
        {
            if(float.TryParse(Text, out Value))
            {
                BackColor = Color.White;
                if(Bind != null)
                    Property.SetValue(Bind, Value);
            }
            else
            {
                BackColor = Color.Red;
            }
        }
    }
}
