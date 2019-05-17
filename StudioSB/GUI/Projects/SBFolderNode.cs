using System.IO;
using System.Windows.Forms;

namespace StudioSB.GUI.Projects
{
    /// <summary>
    /// Represents a folder in the filesystem
    /// </summary>
    public class SBFolderNode : TreeNode
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
            }
        }
        private string _path;

        public SBFolderNode(string FolderPath)
        {
            FilePath = FolderPath;
            AfterCollapse();
        }

        public void BeforeExpand()
        {
            if (IsExpanded) return;
            Nodes.Clear();
            foreach (string s in Directory.GetDirectories(_path + "\\"))
            {
                if (File.GetAttributes(s).HasFlag(FileAttributes.Directory))
                    Nodes.Add(new SBFolderNode(s));
            }
            foreach (string s in Directory.GetFiles(_path + "\\"))
            {
                if (!File.GetAttributes(s).HasFlag(FileAttributes.Directory))
                    Nodes.Add(new SBFileNode(s));
            }
        }

        public void AfterCollapse()
        {
            Nodes.Clear();
            Nodes.Add(new TreeNode("Dummy"));
        }

    }
}
