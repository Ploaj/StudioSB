using System;
using System.Windows.Forms;
using OpenTK;

namespace StudioSB.GUI
{
    /// <summary>
    /// For binding and editing OpenTK Vector4 properties
    /// </summary>
    public class GenericBindingVector4Editor : TableLayoutPanel
    {
        private PropertyBinding<Vector4> _value;

        public float X { get { return _value.Value.X; } set { _value.Value = new Vector4(value, Y, Z, W); } }

        public float Y { get { return _value.Value.Y; } set { _value.Value = new Vector4(X, value, Z, W); } }

        public float Z { get { return _value.Value.Z; } set { _value.Value = new Vector4(X, Y, value, W); } }

        public float W { get { return _value.Value.W; } set { _value.Value = new Vector4(X, Y, Z, value); } }

        private bool IsColor;
        private Label NameLabel;
        private GenericBindingTextBox<float> XEdit;
        private GenericBindingTextBox<float> YEdit;
        private GenericBindingTextBox<float> ZEdit;
        private GenericBindingTextBox<float> WEdit;

        private SBButton ColorSelect;

        public GenericBindingVector4Editor(string Name = "", bool isColor = false) : base()
        {
            _value = new PropertyBinding<Vector4>();

            IsColor = isColor;

            MaximumSize = new System.Drawing.Size(int.MaxValue, 32);

            NameLabel = new Label();
            NameLabel.Text = Name;

            Controls.Add(NameLabel, 0, 0);

            if (isColor)
            {
                ColorSelect = new SBButton("");
                ColorSelect.Click += SelectColor;
                Controls.Add(ColorSelect);
                Controls.Add(ColorSelect, 1, 0);
            }
            else
            {
                XEdit = new GenericBindingTextBox<float>();
                YEdit = new GenericBindingTextBox<float>();
                ZEdit = new GenericBindingTextBox<float>();
                WEdit = new GenericBindingTextBox<float>();
                System.Drawing.Size MaxSize = new System.Drawing.Size(64, 32);
                XEdit.MaximumSize = MaxSize;
                YEdit.MaximumSize = MaxSize;
                ZEdit.MaximumSize = MaxSize;
                WEdit.MaximumSize = MaxSize;
                Controls.Add(XEdit, 1, 0);
                Controls.Add(YEdit, 2, 0);
                Controls.Add(ZEdit, 3, 0);
                Controls.Add(WEdit, 4, 0);
            }

            RowStyles.Add(new RowStyle() { SizeType = SizeType.AutoSize });
        }

        private void SelectColor(object sender, EventArgs args)
        {
            using (ColorDialog dialog = new ColorDialog())
            {
                dialog.Color = ColorSelect.BackColor;
                if(dialog.ShowDialog() == DialogResult.OK)
                {
                    ColorSelect.BackColor = dialog.Color;
                    X = dialog.Color.R / 255f;
                    Y = dialog.Color.G / 255f;
                    Z = dialog.Color.B / 255f;
                    W = dialog.Color.A / 255f;
                }
            }
        }

        public void Bind(object Object, string PropertyName)
        {
            UnBind();
            _value.Bind(Object, PropertyName);
            
            if(IsColor)
                ColorSelect.BackColor = System.Drawing.Color.FromArgb((int)(W * 255), (int)(X * 255), (int)(Y * 255), (int)(Z * 255));
            else
            {
                XEdit.Bind(this, "X");
                YEdit.Bind(this, "Y");
                ZEdit.Bind(this, "Z");
                WEdit.Bind(this, "W");
            }
        }

        public void UnBind()
        {
            _value.UnBind();

            if (!IsColor)
            {
                XEdit.UnBind();
                YEdit.UnBind();
                ZEdit.UnBind();
                WEdit.UnBind();
            }
        }
    }
}
