using System.Windows.Forms;

namespace StudioSB.Tools
{
    public class FileTools
    {
        public static string TryOpenFolder(string title = "")
        {
            using (var dialog = new FolderSelectDialog())
            {
                if (!string.IsNullOrEmpty(title))
                    dialog.Title = title;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    return dialog.SelectedPath;
                }

            }

            return "";
        }

        public static bool TryOpenFile(out string fileName, string filter = "")
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = filter;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    fileName = dialog.FileName;
                    return true;
                }
            }
            fileName = "";
            return false;
        }

        public static bool TrySaveFile(out string fileName, string filter = "", string defaultFileName = "")
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = filter;
                dialog.FileName = defaultFileName;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    fileName = dialog.FileName;
                    return true;
                }
            }
            fileName = "";
            return false;
        }
    }
}