using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace StudioSB.GUI.Projects
{
    /// <summary>
    /// Represents a file in the file system
    /// </summary>
    public class SBFileNode : TreeNode
    {
        public string FilePath
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value;
                Text = Path.GetFileName(_path.Replace(":", ""));
                if (!MainForm.OpenableExtensions.Contains(Path.GetExtension(Text)))
                {
                    ForeColor = Color.Gray;
                }
            }
        }
        private string _path;

        public SBFileNode(string FilePath)
        {
            this.FilePath = FilePath;
        }
    }
}
