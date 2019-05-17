using System.Windows.Forms;

namespace StudioSB
{
    public class SBConsole : TextBox
    {
        public static SBConsole Console
        {
            get
            {
                if (_console == null)
                    _console = new SBConsole();
                return _console;
            }
        }
        private static SBConsole _console;

        public SBConsole()
        {
            ApplicationSettings.SkinControl(this);

            Dock = DockStyle.Fill;
            Multiline = true;
            ScrollBars = ScrollBars.Both;
            ReadOnly = true;
        }

        public static void WriteLine(object line)
        {
            if (_console != null)
            {
                _console.AppendText(line.ToString() + "\r\n");
            }
        }

        public static void WriteLine(string Line = "")
        {
            if(_console != null)
            {
                _console.AppendText(Line + "\r\n");
            }
        }

        public static void SaveToFile(string FilePath = "console.log")
        {
            System.IO.File.WriteAllText(FilePath, _console.Text);
        }
    }
}
