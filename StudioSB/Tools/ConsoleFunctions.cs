using StudioSB.IO;
using StudioSB.Scenes;
using StudioSB.Scenes.Animation;
using System.IO;

namespace StudioSB.Tools
{
    /// <summary>
    /// 
    /// </summary>
    public class ConsoleFunctions
    {
        public static void ProcessCommands(string[] args)
        {
            switch (args[0])
            {
                case "convanim":
                    if(args.Length < 4)
                    {
                        SBConsole.WriteLine("Invalid argument count, expected 4");
                        PrintSupported();
                    }
                    else
                    {
                        ConvertAnim(args[1], args[2], args[3]);
                    }
                    break;
                default:
                    SBConsole.WriteLine($"Unknown command {args[0]}");
                    PrintSupported();
                    break;
            }
        }

        private static void PrintSupported()
        {
            SBConsole.WriteLine("Supported Commands");

            SBConsole.WriteLine("convanim (anim path) (model path) (anim type)");
            SBConsole.WriteLine($"\tSupported Anim Types: {string.Join(", ", MainForm.SupportedAnimExportTypes())}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animPath"></param>
        /// <param name="modelPath"></param>
        /// <param name="output"></param>
        public static void ConvertAnim(string animPath, string modelPath, string output)
        {
            var exporter = MainForm.GetExportableAnimationFromExtension(output);

            if(exporter != null)
            {
                SBConsole.WriteLine($"Converting {animPath} to {Path.ChangeExtension(animPath, output)}");

                var scene = MainForm.LoadScene(modelPath, null);
                if (scene == null)
                {
                    SBConsole.WriteLine("Error Opening Model");
                    return;
                }

                var mod = scene.GetIOModel();
                
                if (Directory.Exists(animPath))
                {
                    foreach (var v in Directory.GetFiles(animPath))
                    {
                        ConvertAnim(v, Path.ChangeExtension(v, output), SBSkeleton.FromIOSkeleton(mod.Models[0].Skeleton), exporter);
                    }
                }
                else
                {
                    ConvertAnim(animPath, Path.ChangeExtension(animPath, output), SBSkeleton.FromIOSkeleton(mod.Models[0].Skeleton), exporter);
                }

            }
            else
            {
                SBConsole.WriteLine($"Unsupported Extension: {output}");
            }
        }

        public static void ConvertAnim(string animPath, string output, SBSkeleton skeleton, IExportableAnimation exporter)
        {
            var anim = MainForm.LoadAnimation(animPath, skeleton);
            if (anim == null)
            {
                SBConsole.WriteLine("Error Opening Animation");
                return;
            }
            ConvertAnim(anim, output, skeleton, exporter);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animation"></param>
        /// <param name="outputPath"></param>
        /// <param name="skeleton"></param>
        /// <param name="animType"></param>
        public static void ConvertAnim(SBAnimation animation, string outputPath, SBSkeleton skeleton, IExportableAnimation animType)
        {
            animType.ExportSBAnimation(outputPath, animation, skeleton);
        }

    }
}
