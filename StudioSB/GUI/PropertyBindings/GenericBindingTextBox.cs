using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace StudioSB.GUI
{
    /// <summary>
    /// A textbox with a bindable value
    /// Note: only bindable with Types that can be parsed from a string
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericBindingTextBox<T> : TextBox
    {
        /// <summary>
        /// The bindable value for this textbox
        /// </summary>
        private PropertyBinding<T> _value;

        public T Value { get => _value.Value; }

        public GenericBindingTextBox()
        {
            ApplicationSettings.SkinControl(this);
            TextChanged += ValueChanged;
            _value = new PropertyBinding<T>();
        }

        /// <summary>
        /// Binds an object's property to this textbox
        /// </summary>
        /// <param name="Object"></param>
        /// <param name="Property"></param>
        public void Bind(object Object, string Property)
        {
            UnBind();
            _value.Bind(Object, Property);

            // so the value isn't updated at this time we unsubscribe from the event temporarily 
            TextChanged -= ValueChanged;
            Text = _value.Value.ToString();
            TextChanged += ValueChanged;
        }
        
        /// <summary>
        /// Binds n class's static field to type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="Property"></param>
        public void Bind(Type type, string Property)
        {
            _value.Bind(type, Property);
            Text = _value.Value.ToString();
        }

        /// <summary>
        /// Unbinds the currently bound object property
        /// </summary>
        public void UnBind()
        {
            _value.UnBind();
        }

        public new void Dispose()
        {
            _value.UnBind();
            base.Dispose();
        }
        
        /// <summary>
        /// Trys to set the value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ValueChanged(object sender, EventArgs args)
        {
            BackColor = ApplicationSettings.BackgroundColor;
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                {
                    _value.Value = (T)converter.ConvertFromString(Text);
                }
                else
                    BackColor = ApplicationSettings.ErrorColor;
            }
            catch (Exception)
            {
                BackColor = ApplicationSettings.ErrorColor;
            }
        }
    }
}
