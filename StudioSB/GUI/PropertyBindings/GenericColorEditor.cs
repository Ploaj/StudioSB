using System;
using System.Drawing;
using System.Windows.Forms;

namespace StudioSB.GUI.Editors
{
    /// <summary>
    /// A color selected that can be binded to an object property
    /// </summary>
    public class GenericColorEditor : SBButton
    {
        private PropertyBinding<Color> _value;

        public GenericColorEditor(string text) : base(text)
        {
            _value = new PropertyBinding<Color>();

            MaximumSize = new Size(128, 32);

            Click += onClick;
        }

        private void onClick(object sender, EventArgs args)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                cd.Color = BackColor;

                if(cd.ShowDialog() == DialogResult.OK)
                {
                    BackColor = cd.Color;
                    ForeColor = ContrastColor(BackColor);
                    _value.Value = BackColor;
                }
            }
        }

        private Color ContrastColor(Color color)
        {
            int d = 0;

            // Counting the perceptive luminance - human eye favors green color... 
            double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;

            if (luminance > 0.5)
                d = 0; // bright colors - black font
            else
                d = 255; // dark colors - white font

            return Color.FromArgb(d, d, d);
        }

        public void Bind(object Object, string PropertyName)
        {
            _value.Bind(Object, PropertyName);
            BackColor = _value.Value;
            ForeColor = ContrastColor(BackColor);
        }

        public void UnBind()
        {
            _value.UnBind();
        }
    }
}
