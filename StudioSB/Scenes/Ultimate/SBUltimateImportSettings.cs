using StudioSB.GUI;
using System.ComponentModel;
using System.Drawing.Design;

namespace StudioSB.Scenes.Ultimate
{
    public class SBUltimateImportSettings
    {
        [Editor(
        typeof(FilteredFileNameEditor),
        typeof(UITypeEditor)),
        DisplayName("NUSKT FilePath"),
        Description("Path to NUSKTB that you want to use. Leave blank if you want to import skeleton from file")]
        public string NUSKTFile { get; set; } = "";
        
        [Editor(
        typeof(FilteredFileNameEditor),
        typeof(UITypeEditor)),
        DisplayName("NUMATL FilePath"),
        Description("Path to NUMATLB that you want to use.")]
        public string NUMATLB { get; set; } = "";
    }
}
