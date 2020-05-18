using StudioSB.Scenes;

namespace StudioSB.IO
{
    public interface IImportableSkeleton
    {
        string Name { get; }
        string Extension { get; }

        void ImportSBSkeleton(string FileName, SBSkeleton skeleton);
    }
}