using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using StudioSB.Scenes;
using StudioSB.Scenes.Animation;

namespace StudioSB.IO.Formats
{
    public class IO_SBAnimation : IExportableAnimation, IImportableAnimation
    {
        public string Name => "SBAnimation";

        public string Extension => ".sbanim";

        public object Settings => null;
        
        public void ExportSBAnimation(string FileName, SBAnimation animation, SBSkeleton skeleton)
        {
            using (StreamWriter w = new StreamWriter(new FileStream(FileName, FileMode.Create)))
            {
                w.WriteLine("#SBAnimation Version 1");

                w.WriteLine($"FrameCount {animation.FrameCount}");

                foreach(var visNode in animation.VisibilityNodes)
                {
                    w.WriteLine($"visibility {visNode.MeshName}");
                    w.WriteLine("{");
                    bool prev = false;
                    foreach(var key in visNode.Visibility.Keys)
                    {
                        if(key.Frame == 0 || prev != key.Value)
                            w.WriteLine($"{key.Frame} : {key.Value}");
                        prev = key.Value;
                    }
                    w.WriteLine("}");
                }
            }
        }

        public SBAnimation ImportSBAnimation(string FileName, SBSkeleton skeleton)
        {
            SBAnimation anim = new SBAnimation();

            using (StreamReader r = new StreamReader(new FileStream(FileName, FileMode.Open)))
            {
                if (r.ReadLine() != "#SBAnimation Version 1")
                    return anim;

                while (!r.EndOfStream)
                {
                    var line = r.ReadLine();
                    var args = line.Trim().Split(' ');

                    switch (args[0])
                    {
                        case "FrameCount":
                            anim.FrameCount = int.Parse(args[1]);
                            break;
                        case "visibility":
                            SBVisibilityAnimation visAnim = new SBVisibilityAnimation();
                            visAnim.MeshName = args[1];
                            var visLien = r.ReadLine();
                            if (visLien == "{")
                            {
                                visLien = r.ReadLine();
                                while (!r.EndOfStream && visLien != "}")
                                {
                                    visLien = visLien.Replace(" ", "");
                                    var frame = visLien.Substring(0, visLien.IndexOf(":")).Trim();
                                    var value = visLien.Substring(visLien.IndexOf(":") + 1, visLien.Length - (visLien.IndexOf(":") + 1)).Trim();
                                    visAnim.Visibility.AddKey(float.Parse(frame), bool.Parse(value));
                                    visLien = r.ReadLine();
                                }
                            }
                            anim.VisibilityNodes.Add(visAnim);
                            break;
                        case "bone":
                            if (args[1].Contains(".anim"))
                            {
                                IO_MayaANIM manim = new IO_MayaANIM();
                                var bones = manim.ImportSBAnimation(Path.GetDirectoryName(FileName) + "/" + args[1], skeleton);
                                anim.TransformNodes.AddRange(bones.TransformNodes);
                            }
                            break;
                    }
                }

            }

            return anim;
        }
    }
}
