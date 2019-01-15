using System;
using StudioSB.Scenes;
using StudioSB.Scenes.Animation;
using SELib;

namespace StudioSB.IO.Formats
{
    public class IO_SEANIM : IExportableAnimation
    {
        public string Name { get; } = "SEAnim";
        public string Extension { get; } = ".seanim";

        public void ExportSBAnimation(string FileName, SBAnimation animation, SBSkeleton skeleton)
        {
            SEAnim seOut = new SEAnim(); //init new SEAnim

            foreach (var node in animation.TransformNodes) //iterate through each node
            {
                for (int i = 0; i < animation.FrameCount; i++)
                {
                    SBBone temp = new SBBone();
                    temp.Transform = node.Transform.GetValue(i);

                    OpenTK.Vector3 pos = temp.Translation;
                    OpenTK.Quaternion rot = temp.RotationQuaternion;
                    OpenTK.Vector3 sca = temp.Scale;

                    seOut.AddTranslationKey(node.Name, i, pos.X, pos.Y, pos.Z);
                    seOut.AddRotationKey(node.Name, i, rot.X, rot.Y, rot.Z, rot.W);
                    seOut.AddScaleKey(node.Name, i, sca.X, sca.Y, sca.Z);
                }
            }

            seOut.Write(FileName); //write it!
        }
    }
}
