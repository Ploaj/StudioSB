using StudioSB.Scenes;
using StudioSB.Scenes.Animation;
using SELib;

namespace StudioSB.IO.Formats
{
    public class IO_SEANIM : IExportableAnimation
    {
        public string Name { get; } = "SEAnim";
        public string Extension { get; } = ".seanim";
        public object Settings { get; } = null;

        public void ExportSBAnimation(string FileName, SBAnimation animation, SBSkeleton skeleton)
        {
            SEAnim seOut = new SEAnim(); //init new SEAnim

            foreach (var node in animation.TransformNodes) //iterate through each node
            {
                for (int i = 0; i < animation.FrameCount; i++)
                {
                    SBBone temp = new SBBone();

                    temp.Transform = node.GetTransformAt(i, skeleton);

                    seOut.AddTranslationKey(node.Name, i, temp.X, temp.Y, temp.Z);
                    seOut.AddRotationKey(node.Name, i, temp.RotationQuaternion.X, temp.RotationQuaternion.Y, temp.RotationQuaternion.Z, temp.RotationQuaternion.W);
                    seOut.AddScaleKey(node.Name, i, temp.SX, temp.SY, temp.SZ);
                }
            }

            seOut.Write(FileName); //write it!
        }
    }
}
