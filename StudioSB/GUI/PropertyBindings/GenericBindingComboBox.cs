using System;
using System.Windows.Forms;

namespace StudioSB.GUI
{
    /// <summary>
    /// A combo box with bindings for an Enum value type
    /// </summary>
    public class GenericBindingComboBox<T> : ComboBox
    {
        private PropertyBinding<T> _value;

        public GenericBindingComboBox(string text) : base()
        {
            ApplicationSettings.SkinControl(this);
            
            _value = new PropertyBinding<T>();
            DropDownStyle = ComboBoxStyle.DropDownList;

            SelectedIndexChanged += changeItem;
        }
        
        private void changeItem(object sender, EventArgs args)
        {
            _value.Value = (T)SelectedItem;
        }

        /// <summary>
        /// Binds an objects enum property to this combobox
        /// </summary>
        /// <param name="Object"></param>
        /// <param name="PropertyName"></param>
        public void Bind(object Object, string PropertyName)
        {
            _value.Bind(Object, PropertyName);
            Items.Clear();

            if (_value.IsBound)
            {
                if (typeof(T).IsValueType)
                {
                    foreach (var e in (T[])Enum.GetValues(typeof(T)))
                    {
                        Items.Add(e);
                    }
                }

                SelectedItem = _value.Value;
            }
        }

        /// <summary>
        /// Unbinds the object property from this combobox
        /// </summary>
        public void UnBind()
        {
            _value.UnBind();
            Items.Clear();
        }
    }
}
