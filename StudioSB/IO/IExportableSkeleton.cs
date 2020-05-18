using StudioSB.Scenes;
using StudioSB.Scenes.Animation;

namespace StudioSB.IO
{
    public interface IExportableSkeleton
    {
        string Name { get; }
        string Extension { get; }

        void ExportSBSkeleton(string FileName, SBSkeleton skeleton);
    }
}