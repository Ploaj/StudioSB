using StudioSB.Scenes;
using StudioSB.Scenes.Animation;

namespace StudioSB.IO
{
    public interface IExportableAnimation
    {
        string Name { get; }
        string Extension { get; }
        object Settings { get; }

        void ExportSBAnimation(string FileName, SBAnimation animation, SBSkeleton skeleton);
    }
}
