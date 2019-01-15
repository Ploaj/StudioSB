using System;
using System.Windows.Forms;

namespace StudioSB.GUI.Editors
{
    public class EditorInfo : Attribute
    {
        public string Name;
        public string Description;
        public bool IsColor;
    }

    /// <summary>
    /// And editor for generically editing members of class
    /// </summary>
    public class GenericEditor : Panel
    {

    }
}
