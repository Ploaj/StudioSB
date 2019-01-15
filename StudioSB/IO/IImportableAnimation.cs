using StudioSB.Scenes;
using StudioSB.Scenes.Animation;

namespace StudioSB
{
    public interface IImportableAnimation
    {
        string Name { get; }
        string Extension { get; }

        SBAnimation ImportSBAnimation(string FileName, SBSkeleton skeleton);
    }
}
