using StudioSB.Tools;
using System;
using System.Windows.Forms;

namespace StudioSB
{
    public class SBConsole : Panel
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

        private TextBox Output;
        private TextBox Input;

        public SBConsole()
        {
            ApplicationSettings.SkinControl(this);

            Dock = DockStyle.Fill;

            Output = new TextBox();
            Output.Multiline = true;
            Output.ScrollBars = ScrollBars.Both;
            Output.ReadOnly = true;
            Output.Dock = DockStyle.Fill;
            ApplicationSettings.SkinControl(Output);

            Input = new TextBox();
            Input.Dock = DockStyle.Bottom;
            Input.KeyPress += (sender, args) =>
            {
                if(args.KeyChar == (char)13)
                {
                    WriteLine(Input.Text);
                    ConsoleFunctions.ProcessCommands(SplitArguments(Input.Text));
                    Input.Text = "";
                }
            };
            ApplicationSettings.SkinControl(Input);

            Controls.Add(Output);
            Controls.Add(Input);
        }

        public static string[] SplitArguments(string commandLine)
        {
            var parmChars = commandLine.ToCharArray();
            var inSingleQuote = false;
            var inDoubleQuote = false;
            for (var index = 0; index < parmChars.Length; index++)
            {
                if (parmChars[index] == '"' && !inSingleQuote)
                {
                    inDoubleQuote = !inDoubleQuote;
                    parmChars[index] = '\n';
                }
                if (parmChars[index] == '\'' && !inDoubleQuote)
                {
                    inSingleQuote = !inSingleQuote;
                    parmChars[index] = '\n';
                }
                if (!inSingleQuote && !inDoubleQuote && parmChars[index] == ' ')
                    parmChars[index] = '\n';
            }
            return (new string(parmChars)).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private void AppendText(string text)
        {
            Output.AppendText(text);
        }

        public static void WriteLine(object line)
        {
            if (_console != null)
            {
                _console.AppendText(line.ToString() + "\r\n");
            }
            else
            {
                System.Console.WriteLine(line);
            }
        }

        public static void WriteLine(string Line = "")
        {
            if(_console != null)
            {
                _console.AppendText(Line + "\r\n");
            }
            else
            {
                System.Console.WriteLine(Line);
            }
        }

        public static void SaveToFile(string FilePath = "console.log")
        {
            System.IO.File.WriteAllText(FilePath, _console.Text);
        }
    }
}
