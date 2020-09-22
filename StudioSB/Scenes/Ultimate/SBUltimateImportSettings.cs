using StudioSB.GUI;
using System.ComponentModel;
using System.Drawing.Design;

namespace StudioSB.Scenes.Ultimate
{
    public class SBUltimateImportSettings
    {
        [DisplayName("Use Existing Skeleton"),
        Description("Uses the already existing skeleton instead of the imported one (Recommended)")]
        public bool UseExistingSkeleton { get; set; } = true;     
    }

    public class SBUltimateNewImportSettings
    {
        [Editor(
        typeof(FilteredFileNameEditor),
        typeof(UITypeEditor)),
        DisplayName("NUSKTB FilePath"),
        Description("Path to the .nusktb that you want to use. Leave blank if you want to import skeleton from file")]
        public string NusktbFile { get; set; } = "";
        
        [Editor(
        typeof(FilteredFileNameEditor),
        typeof(UITypeEditor)),
        DisplayName("NUMATB FilePath"),
        Description("Path to the .numatb that you want to use.")]
        public string NumatbFile { get; set; } = "";
    }
}
